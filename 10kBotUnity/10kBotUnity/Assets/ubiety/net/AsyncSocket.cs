// AsyncSocket.cs
//
//Ubiety XMPP Library Copyright (C) 2006 - 2015 Dieter Lunn
//
//This library is free software; you can redistribute it and/or modify it under
//the terms of the GNU Lesser General Public License as published by the Free
//Software Foundation; either version 3 of the License, or (at your option)
//any later version.
//
//This library is distributed in the hope that it will be useful, but WITHOUT
//ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
//FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more
//
//You should have received a copy of the GNU Lesser General Public License along
//with this library; if not, write to the Free Software Foundation, Inc., 59
//Temple Place, Suite 330, Boston, MA 02111-1307 USA

using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using Ubiety.Common;
using Ubiety.Infrastructure;
using Ubiety.Infrastructure.Extensions;
using Ubiety.Registries;
using Ubiety.States;
using UnityEngine;

namespace Ubiety.Net
{
    /// <remarks>
    ///     AsyncSocket is the class that communicates with the server.
    /// </remarks>
    internal class AsyncSocket : IDisposable
    {
        // Timeout after 5 seconds by default
        /*
                private const int Timeout = 5000;
        */
        private const int BufferSize = 4096;
        private readonly byte[] _bufferBytes = new byte[BufferSize];
        private readonly Address _destinationAddress;
        private readonly ManualResetEvent _timeoutEvent = new ManualResetEvent(false);
        private readonly UTF8Encoding _utf = new UTF8Encoding();
        private bool _compressed;
        private ICompression _compression;
        private Socket _socket;
        private Stream _stream;

        public AsyncSocket()
        {
            _destinationAddress = new Address();
            ProtocolState.Events.OnSend += Events_OnSend;
        }

        private void Events_OnSend(object sender, TagEventArgs e)
        {
            Write(e.Tag.ToString());
        }

        #region Properties

        /// <summary>
        ///     Gets the current status of the socket.
        /// </summary>
        public bool Connected { get; private set; }

        /*
                /// <summary>
                /// </summary>
                public string Hostname
                {
                    get { return _destinationAddress.Hostname; }
                }
        */

        /*
                /// <summary>
                /// </summary>
                public bool Secure { get; set; }
        */

        #endregion

        public void Dispose()
        {
            LogQueue.Warn("Why can't I dispose this?");
            // _timeoutEvent.Dispose();
            // _socket.Dispose();
        }

        /// <summary>
        ///     Establishes a connection to the specified remote host.
        /// </summary>
        /// <returns>True if we connected, false if we didn't</returns>
        public void Connect()
        {
            LogQueue.Log("AsyncSocket.Connect() is trying hostname " + _destinationAddress.Hostname);
            var address = _destinationAddress.NextIpAddress();
            LogQueue.Log("AsyncSocket.Connect() is trying hostname " + _destinationAddress.Hostname + " address " + address);
            IPEndPoint end;
            if (address != null)
            {
                end = new IPEndPoint(address, ProtocolState.Settings.Port);
                LogQueue.Log("AsyncSocket.Connect got IPEndPoint " + end);
            }
            else
            {
                ProtocolState.Events.Error(this, ErrorType.ConnectionTimeout, ErrorSeverity.Fatal,
                    "Unable to obtain server IP address.");
                return;
            }


            _socket = !_destinationAddress.IPv6
                ? new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                : new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                _socket.BeginConnect(end, FinishConnect, _socket);
                //if (!_timeoutEvent.WaitOne(Timeout))
                //{
                //    Errors.Instance.SendError(this, ErrorType.ConnectionTimeout, "Timed out connecting to server.");
                //    return;
                //}
            }
            catch (SocketException e)
            {
                LogQueue.Error("Error in connecting socket. Got " + e);
                ProtocolState.Events.Error(this, ErrorType.ConnectionTimeout, ErrorSeverity.Fatal,
                    "Unable to connect to server.");
            }
        }

        private void FinishConnect(IAsyncResult ar)
        {
            LogQueue.Log("AsyncSocket.FinishConnect(" + ar + "). IsCompleted? " + ar.IsCompleted);
            try
            {
                var socket = (Socket)ar.AsyncState;
                socket.EndConnect(ar);

                Connected = true;

                var netstream = new NetworkStream(socket);
                _stream = netstream;

                _stream.BeginRead(_bufferBytes, 0, BufferSize, Receive, null);

                ProtocolState.State = new ConnectedState();
                ProtocolState.State.Execute();
            }
            catch (Exception e)
            {
                LogQueue.Warn(e + " occured in AsyncSocket.FinishConnect");
            }
            finally
            {
                _timeoutEvent.Set();
            }
        }

        /// <summary>
        ///     Disconnects the socket from the server.
        /// </summary>
        public void Disconnect()
        {
            Connected = false;
            _stream.Close();
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Disconnect(true);
        }

        /// <summary>
        ///     Encrypts the connection using SSL/TLS
        /// </summary>
        public void StartSecure()
        {
            RemoteCertificateValidationCallback previous = ServicePointManager.ServerCertificateValidationCallback;
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback((o, xc, xch, spe) => { return true; });
            var sslstream = new SslStream(_stream, true, RemoteValidation);
            try
            {
                LogQueue.Log("Attempting to authenticate with " + _destinationAddress.Hostname);
                sslstream.AuthenticateAsClient(_destinationAddress.Hostname, null, SslProtocols.Tls, false);
                if (sslstream.IsAuthenticated)
                {
                    LogQueue.Log("sslstream has authenticated with " + _destinationAddress.Hostname);
                    _stream = sslstream;
                }
                else
                {
                    LogQueue.Warn("sslstream has failed authentication for some reason.");
                    // sslstream.
                }
            }
            catch (Exception e)
            {
                LogQueue.Error("Error starting secure connection. Received " + e);
                ProtocolState.Events.Error(this, ErrorType.XmlError, ErrorSeverity.Fatal, "Cannot connect with SSL.");
            }
            finally
            {
                ServicePointManager.ServerCertificateValidationCallback = previous;
            }
        }

        private static bool RemoteValidation(object sender, X509Certificate cert, X509Chain chain,
            SslPolicyErrors errors)
        {
            LogQueue.Log("AsyncSocket.RemoteValidation(" + sender + ", " + cert + ", " + chain + ", " + errors + ") is running.");
            if (errors == SslPolicyErrors.None)
            {
                LogQueue.Log("Passed remote validation!");
                return true;
            }

            if (cert.Subject.ToLower().Contains("livecoding.tv"))
            {
                LogQueue.Log("Seeing that cert came from livecoding.tv. Returning true.");
                return true;
            }

            if ((errors & SslPolicyErrors.RemoteCertificateChainErrors) != 0)
            {
                LogQueue.Error("Chain error. chain is null: " + (chain == null));
                if (chain != null)
                {
                    LogQueue.Log("ChainPolicy: " + chain.ChainPolicy);
                    for (int n = 0; n < chain.ChainStatus.Length; n++)
                    {
                        LogQueue.Log("ChainStatus[" + n + "] is " + chain.ChainStatus[n].Status + " with info " + chain.ChainStatus[n].StatusInformation);
                    }
                    for (int n = 0; n < chain.ChainElements.Count; n++)
                    {
                        LogQueue.Log("ChainElements[" + n + "] has status " + chain.ChainElements[n].ChainElementStatus + " and info " + chain.ChainElements[n].Information);
                    }
                }
            }

            if ((errors & SslPolicyErrors.RemoteCertificateNotAvailable) != 0)
            {
                LogQueue.Error("Got remove cert not available error. cert is null: " + (cert == null));
                if (cert != null)
                {
                    LogQueue.Log("cert subject " + cert.Subject + ", expires " + cert.GetExpirationDateString());

                }
            }

            LogQueue.Warn("Received SslPolicyErrors " + errors);
            return false;
        }

        /// <summary>
        ///     Writes data to the current connection.
        /// </summary>
        /// <param name="msg">Message to send</param>
        public void Write(string msg)
        {
            LogQueue.Log("Outgoing message: " + msg);

            if (!Connected)
            {
                LogQueue.Warn("Failed to send message because we are not connected.");
                return;
            }
            var mesg = _utf.GetBytes(msg);
            mesg = _compressed ? _compression.Deflate(mesg) : mesg;
            try
            {
                LogQueue.Log("Writing message to stream: " + msg);
                _stream.Write(mesg, 0, mesg.Length);
            }
            catch (Exception e)
            {
                LogQueue.Error("Failed to send message " + msg + ". Received " + e + ": " + e.Message);
            }
        }

        private void Receive(IAsyncResult ar)
        {
            try
            {
                _stream.EndRead(ar);

                var t = _bufferBytes.TrimNull();

                var m = _utf.GetString(_compressed ? _compression.Inflate(t, t.Length) : t);

                LogQueue.Log("Incoming Message: " + m);
                ProtocolParser.Parse(m);

                // Clear the buffer otherwise we get leftover tags and it confuses the parser.
                _bufferBytes.Clear();

                if (!Connected || ProtocolState.State is DisconnectedState) return;

                _stream.BeginRead(_bufferBytes, 0, _bufferBytes.Length, Receive, null);
            }
            catch (SocketException e)
            {
                LogQueue.Error("Error in socket receiving data. Received " + e);
            }
            catch (InvalidOperationException e)
            {
                LogQueue.Error("Socket committed an invalid operation trying to receive data. Received " + e);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="algorithm"></param>
        public void StartCompression(string algorithm)
        {
            _compression = CompressionRegistry.GetCompression(algorithm);
            _compressed = true;
        }
    }
}