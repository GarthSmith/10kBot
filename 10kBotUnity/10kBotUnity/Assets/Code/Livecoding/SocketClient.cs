﻿/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 * Copyright (c) 2003-2008 by AG-Software 											 *
 * All Rights Reserved.																 *
 * Contact information for AG-Software is available at http://www.ag-software.de	 *
 *																					 *
 * Licence:																			 *
 * The agsXMPP SDK is released under a dual licence									 *
 * agsXMPP can be used under either of two licences									 *
 * 																					 *
 * A commercial licence which is probably the most appropriate for commercial 		 *
 * corporate use and closed source projects. 										 *
 *																					 *
 * The GNU Public License (GPL) is probably most appropriate for inclusion in		 *
 * other open source projects.														 *
 *																					 *
 * See README.html for details.														 *
 *																					 *
 * For general enquiries visit our website at:										 *
 * http://www.ag-software.de														 *
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Text;
using System.Configuration;
using System.Collections;
using System.Diagnostics;

using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace UnitySocketTest
{
    // using Mono.Security.Protocol.Tls;

    public class ConnectTimeoutException : Exception
    {
        public ConnectTimeoutException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// Use async sockets to connect, send and receive data over TCP sockets.
    /// </summary>
    public class ClientSocket
    {
        Socket _socket;
        SslStream m_SSLStream;
#if MONOSSL
        SslClientStream		m_SSLStream;
#endif
        NetworkStream m_Stream;
        Stream m_NetworkStream = null;


        const int BUFFERSIZE = 1024;
        private byte[] m_ReadBuffer = null;

        private bool m_SSL = false;

        private bool m_PendingSend = false;
        private Queue m_SendQueue = new Queue();

        /// <summary>
        /// is compression used for this connection
        /// </summary>
        private bool m_Compressed = false;

        private bool m_ConnectTimedOut = false;
        /// <summary>
        /// is used to compress data
        /// </summary>
        // private Deflater deflater = null;
        /// <summary>
        /// is used to decompress data
        /// </summary>
        // private Inflater inflater = null;

        private Timer connectTimeoutTimer;


        #region << Constructor >>
        public ClientSocket()
        {

        }
        #endregion

        #region << Properties >>
        public bool SSL
        {
            get { return m_SSL; }
            set { m_SSL = value; }
        }

        public const bool SupportsStartTls = true;

        /// <summary>
        /// Returns true if the socket is connected to the server. The property 
        /// Socket.Connected does not always indicate if the socket is currently 
        /// connected, this polls the socket to determine the latest connection state.
        /// </summary>
        public bool Connected
        {
            get
            {
                // return right away if have not created socket
                if (_socket == null)
                    return false;

                return _socket.Connected;

                // commented this out because it caused problems on some machines.
                // return the connected property of the socket now

                //the socket is not connected if the Connected property is false
                //if (!_socket.Connected)
                //    return false;

                //// there is no guarantee that the socket is connected even if the
                //// Connected property is true
                //try
                //{
                //    // poll for error to see if socket is connected
                //    return !_socket.Poll(1, SelectMode.SelectError);
                //}
                //catch
                //{
                //    return false;
                //}
            }
        }

        public bool Compressed
        {
            get { return m_Compressed; }
            set { m_Compressed = value; }
        }
        #endregion

        /// <summary>
        /// Connect to the specified address and port number.
        /// </summary>
        public void Connect(string address, int port)
        {
            Address = address;
            Port = port;

            Connect();
        }
        public string Address;
        public int Port;

        public void Connect()
        {
            UnityEngine.Debug.Log("SocketClient.Connect() is running.");
            // Socket is never compressed at startup
            m_Compressed = false;

            m_ReadBuffer = null;
            m_ReadBuffer = new byte[BUFFERSIZE];

            try
            {
                IPHostEntry ipHostInfo = Dns.GetHostEntry(Address);
                IPAddress ipAddress = ipHostInfo.AddressList[0];// IPAddress.Parse(address);
                IPEndPoint endPoint = new IPEndPoint(ipAddress, Port);

                // Timeout
                // .NET supports no timeout for connect, and the default timeout is very high, so it could
                // take very long to establish the connection with the default timeout. So we handle custom
                // connect timeouts with a timer
                m_ConnectTimedOut = false;
                TimerCallback timerDelegate = new TimerCallback(connectTimeoutTimerDelegate);
                connectTimeoutTimer = new Timer(timerDelegate, null, ConnectTimeout, ConnectTimeout);

                // IPV6 Support for .NET 2.0
                if (Socket.OSSupportsIPv6 && (endPoint.AddressFamily == AddressFamily.InterNetworkV6))
                {
                    //Debug.WriteLine("IPV6");
                    _socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
                }
                else
                {
                    //Debug.WriteLine("IPV4");
                    _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                }

                UnityEngine.Debug.LogWarning("Socket is opening a connection. This must be shut off or else it will block future threads!");
                _socket.BeginConnect(endPoint, new AsyncCallback(EndConnect), null);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.Log("Got " + ex);
            }
        }

        private void EndConnect(IAsyncResult ar)
        {
            if (m_ConnectTimedOut)
            {
                UnityEngine.Debug.Log("Timed out.");
                // base.FireOnError(new ConnectTimeoutException("Attempt to connect timed out"));
            }
            else
            {
                try
                {
                    // stop the timeout timer
                    connectTimeoutTimer.Dispose();

                    // pass connection status with event
                    _socket.EndConnect(ar);

                    m_Stream = new NetworkStream(_socket, false);

                    m_NetworkStream = m_Stream;

                    if (m_SSL)
                        InitSSL();

                    // UnityEngine.Debug.Log("Connected!");
                    // FireOnConnect();

                    // Setup Receive Callback
                    this.Receive();
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.Log("Got " + ex);
                }
            }
        }

        /// <summary>
        /// Connect Timeout Timer Callback
        /// </summary>
        /// <param name="stateInfo"></param>
        private void connectTimeoutTimerDelegate(Object stateInfo)
        {
            // for compression debug statisticsUnityEngine.Debug.Log("Connect Timeout");
            connectTimeoutTimer.Dispose();
            m_ConnectTimedOut = true;
            // This must happen each time we connect a socket.
            UnityEngine.Debug.LogWarning("Closing socket!");
            _socket.Close();
        }

        /// <summary>
        /// Starts TLS on a "normal" connection
        /// </summary>
        public void StartTls()
        {
            SslProtocols protocol = SslProtocols.Tls;
            InitSSL(protocol);
        }

        /// <summary>
        /// 
        /// </summary>
        private void InitSSL()
        {
            InitSSL(SslProtocols.Default);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="protocol"></param>		
        private void InitSSL(SslProtocols protocol)
        {
            m_SSLStream = new SslStream(
                m_Stream,
                false,
                new RemoteCertificateValidationCallback(ValidateCertificate),
                null
                );
            try
            {
                m_SSLStream.AuthenticateAsClient(Address, null, protocol, true);
                // Display the properties and settings for the authenticated stream.
                //DisplaySecurityLevel(m_SSLStream);
                //DisplaySecurityServices(m_SSLStream);
                //DisplayCertificateInformation(m_SSLStream);
                //DisplayStreamProperties(m_SSLStream);

            }
            catch (AuthenticationException e)
            {
                //UnityEngine.Debug.Log("Exception: {0}", e.Message);
                if (e.InnerException != null)
                {
                    //UnityEngine.Debug.Log("Inner exception: {0}", e.InnerException.Message);
                }
                //Console.WriteLine ("Authentication failed - closing the connection.");
                //client.Close();
                return;
            }

            m_NetworkStream = m_SSLStream;
            m_SSL = true;
        }


        #region << SSL Properties Display stuff >>

        private void DisplaySecurityLevel(SslStream stream)
        {
            UnityEngine.Debug.Log("Cipher: " + stream.CipherAlgorithm + " strength " + stream.CipherStrength);
            UnityEngine.Debug.Log("Hash: " + stream.HashAlgorithm + " strength " + stream.HashStrength);
            //  UnityEngine.Debug.Log("Key exchange: {0} strength {1}", stream.KeyExchangeAlgorithm, stream.KeyExchangeStrength);
            // UnityEngine.Debug.Log("Protocol: {0}", stream.SslProtocol);
        }

        private void DisplaySecurityServices(SslStream stream)
        {
            UnityEngine.Debug.Log("Is authenticated: " + stream.IsAuthenticated + " as server? " + stream.IsServer);
            // UnityEngine.Debug.Log("IsSigned: {0}", stream.IsSigned);
            //UnityEngine.Debug.Log("Is Encrypted: {0}", stream.IsEncrypted);
        }

        private void DisplayStreamProperties(SslStream stream)
        {
            UnityEngine.Debug.Log("Can read: " + stream.CanRead + " , write " + stream.CanWrite);
            // UnityEngine.Debug.Log("Can timeout: {0}", stream.CanTimeout);
        }

        private void DisplayCertificateInformation(SslStream stream)
        {
            //UnityEngine.Debug.Log("Certificate revocation list checked: {0}", stream.CheckCertRevocationStatus);
            // Display the properties of the client's certificate.
            X509Certificate remoteCertificate = stream.RemoteCertificate;
            if (remoteCertificate != null)
            {
                UnityEngine.Debug.Log("Some cert info should've displayed here.");
                /*UnityEngine.Debug.Log("Remote cert was issued to {0} and is valid from {1} until {2}.",
                    remoteCertificate.Subject,
                    remoteCertificate.GetEffectiveDateString(),
                    remoteCertificate.GetExpirationDateString());
                    */
            }
            else
            {
                UnityEngine.Debug.Log("Remote certificate is null.");
            }
        }

        #endregion

        /// <summary>
        /// Validate the SSL certificate here
        /// for now we dont stop the SSL connection an return always true
        /// </summary>
        /// <param name="certificate"></param>
        /// <param name="certificateErrors"></param>
        /// <returns></returns>
        //private bool ValidateCertificate (X509Certificate certificate, int[] certificateErrors) 
        private bool ValidateCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
#if MONOSSL
        /// <summary>
		/// Starts TLS on a "normal" connection
		/// </summary>
		public override void StartTls()
		{
			base.StartTls();

			Mono.Security.Protocol.Tls.SecurityProtocolType protocol = Mono.Security.Protocol.Tls.SecurityProtocolType.Tls;
			InitSSL(protocol);
		}

		/// <summary>
		/// 
		/// </summary>
		private void InitSSL()
		{
			InitSSL(Mono.Security.Protocol.Tls.SecurityProtocolType.Default);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="protocol"></param>
		private void InitSSL(Mono.Security.Protocol.Tls.SecurityProtocolType protocol)
		{
			m_SSLStream = new SslClientStream(m_Stream, Address, false, protocol, null);				
			m_SSLStream.ServerCertValidationDelegate = new Mono.Security.Protocol.Tls.CertificateValidationCallback(ValidateCertificate);
			m_NetworkStream = m_SSLStream;	
			m_SSL = true;
			// Send a whitespace to start the encryption of the connection now
			Send(" ");
		}

		/// <summary>
		/// Validate the SSL certificate here
		/// for now we dont stop the SSL connection an return always true
		/// </summary>
		/// <param name="certificate"></param>
		/// <param name="certificateErrors"></param>
		/// <returns></returns>
		private bool ValidateCertificate (X509Certificate certificate, int[] certificateErrors) 
		{
			return base.FireOnValidateCertificate(certificate, certificateErrors);
		}
#endif
#if BCCRYPTO
        /// <summary>
        /// Starts TLS on a "normal" connection
        /// </summary>
        public override void StartTls()
        {
            base.StartTls();

            //TlsProtocolHandler protocolHandler = new TlsProtocolHandler(m_NetworkStream, m_NetworkStream);
            //Stream st = new NetworkStream(_socket, false);
            TlsProtocolHandler protocolHandler = new TlsProtocolHandler(m_Stream, m_Stream);
            //TlsProtocolHandler protocolHandler = new TlsProtocolHandler(st, st);

            CertificateVerifier certVerify = new CertificateVerifier();
            certVerify.OnVerifyCertificate += new CertificateValidationCallback(certVerify_OnVerifyCertificate);

            protocolHandler.Connect(certVerify);

            m_NetworkStream = new SslStream(protocolHandler.InputStream, protocolHandler.OutputStream);
            m_SSL = true;
        }

        internal bool certVerify_OnVerifyCertificate(Org.BouncyCastle.Asn1.X509.X509CertificateStructure[] certs)
        {
            return base.FireOnValidateCertificate(certs);
        }
#endif

        /// <summary>
        /// Start Compression on the socket
        /// </summary>
        public void StartCompression()
        {
            InitCompression();
        }

        /// <summary>
        /// Initialize compression stuff (Inflater, Deflater)
        /// </summary>
        private void InitCompression()
        {
            throw new NotImplementedException();
            // base.StartCompression();

            // inflater = new Inflater();
            // deflater = new Deflater();

            // Set the compressed flag to true when we init compression
            // m_Compressed = true;
        }

        /// <summary>
        /// Disconnect from the server.
        /// </summary>
        public void Disconnect()
        {
            // base.Disconnect();

            lock (this)
            {
                // TODO maybe we should notify the user which packets were not sent.
                m_PendingSend = false;
                m_SendQueue.Clear();
            }

            // return right away if have not created socket
            if (_socket == null)
                return;

            try
            {
                // first, shutdown the socket
                _socket.Shutdown(SocketShutdown.Both);
            }
            catch { }

            try
            {
                // next, close the socket which terminates any pending
                // async operations
                _socket.Close();
            }
            catch { }

            UnityEngine.Debug.Log("Disconnected.");
            // FireOnDisconnect();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public void Send(string data)
        {
            Send(Encoding.UTF8.GetBytes(data));
        }

        /// <summary>
        /// Send data to the server.
        /// </summary>
        public void Send(byte[] bData)
        {
            lock (this)
            {
                try
                {
                    // base.FireOnSend(bData, bData.Length);

                    UnityEngine.Debug.Log("Socket OnSend: " + System.Text.Encoding.UTF8.GetString(bData, 0, bData.Length));

                    // compress bytes if we are on a compressed socket
                    if (m_Compressed)
                    {
                        throw new NotImplementedException();
                        // Add inflater deflater.
                        /*
                        byte[] tmpData = new byte[bData.Length];
                        bData.CopyTo(tmpData, 0);

                        bData = Compress(bData);
                        */
                    }

                    // .NET 2.0 SSL Stream issues when sending multiple async packets
                    // http://forums.microsoft.com/MSDN/ShowPost.aspx?PostID=124213&SiteID=1
                    if (m_PendingSend)
                    {
                        m_SendQueue.Enqueue(bData);
                    }
                    else
                    {
                        m_PendingSend = true;
                        try
                        {
                            m_NetworkStream.BeginWrite(bData, 0, bData.Length, new AsyncCallback(EndSend), null);
                        }
                        catch (Exception)
                        {
                            Disconnect();
                        }
                    }
                }
                catch (Exception)
                {

                }
            }

        }

        /// <summary>
        /// Read data from server.
        /// </summary>
        private void Receive()
        {
            m_NetworkStream.BeginRead(m_ReadBuffer, 0, BUFFERSIZE, new AsyncCallback(EndReceive), null);
        }

        private void EndReceive(IAsyncResult ar)
        {
            try
            {
                int nBytes;
                nBytes = m_NetworkStream.EndRead(ar);
                if (nBytes > 0)
                {
                    // uncompress Data if we are on a compressed socket
                    if (m_Compressed)
                    {
                        throw new NotImplementedException();
                        // Need to add inflater and deflater
                        // byte[] buf = Decompress(m_ReadBuffer, nBytes);
                        // base.FireOnReceive(buf, buf.Length);
                    }
                    else
                    {
                        //UnityEngine.Debug.Log("Socket OnReceive: " + System.Text.Encoding.UTF8.GetString(m_ReadBuffer, 0, nBytes));                        
                        // Raise the receive event
                        UnityEngine.Debug.Log("Received something.");
                        // base.FireOnReceive(m_ReadBuffer, nBytes);
                    }
                    // Setup next Receive Callback
                    if (this.Connected)
                        this.Receive();
                }
                else
                {
                    Disconnect();
                }
            }
            catch (ObjectDisposedException)
            {
                //object already disposed, just exit
                return;
            }
            catch (System.IO.IOException ex)
            {
                UnityEngine.Debug.Log("\nSocket Exception: " + ex.Message);
                Disconnect();
            }
        }

        private void EndSend(IAsyncResult ar)
        {
            lock (this)
            {
                try
                {
                    m_NetworkStream.EndWrite(ar);
                    if (m_SendQueue.Count > 0)
                    {
                        byte[] bData = (byte[])m_SendQueue.Dequeue();
                        m_NetworkStream.BeginWrite(bData, 0, bData.Length, new AsyncCallback(EndSend), null);
                    }
                    else
                    {
                        m_PendingSend = false;
                    }
                }
                catch (Exception)
                {
                    Disconnect();
                }
            }
        }

        public long ConnectTimeout = 10000;

        #region << compression functions >>
        /*
        /// <summary>
        /// Compress bytes
        /// </summary>
        /// <param name="bIn"></param>
        /// <returns></returns>
        private byte[] Compress(byte[] bIn)
        {
            int ret;

            // The Flush SHOULD be after each STANZA
            // The libds sends always one complete XML Element/stanza,
            // it doesn't cache stanza and send them in groups, and also doesnt send partial
            // stanzas. So everything should be ok here.
            deflater.SetInput(bIn);
            deflater.Flush();

            MemoryStream ms = new MemoryStream();
            do
            {
                byte[] buf = new byte[BUFFERSIZE];
                ret = deflater.Deflate(buf);
                if (ret > 0)
                    ms.Write(buf, 0, ret);

            } while (ret > 0);

            return ms.ToArray();

        }

        /// <summary>
        /// Decompress bytes
        /// </summary>
        /// <param name="bIn"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        private byte[] Decompress(byte[] bIn, int length)
        {
            int ret;

            inflater.SetInput(bIn, 0, length);

            MemoryStream ms = new MemoryStream();
            do
            {
                byte[] buf = new byte[BUFFERSIZE];
                ret = inflater.Inflate(buf);
                if (ret > 0)
                    ms.Write(buf, 0, ret);

            } while (ret > 0);

            return ms.ToArray();
        }
        */
        #endregion

    }
}