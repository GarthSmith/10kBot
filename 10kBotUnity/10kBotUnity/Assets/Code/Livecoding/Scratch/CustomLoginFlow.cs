using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using UnityEngine;

public class CustomLoginFlow : MonoBehaviour
{
    void Awake()
    {
        Client = new TcpClient(ServerName, 5222);
    }

    void OnEnable()
    {
        // IPEndPoint livecodingEndPoint = new IPEndPoint(;
        // Client.Connect();
        try
        {
            Stream = Client.GetStream();
            Debug.Log("Client has Connected? " + Client.Connected);
        }
        catch (Exception e)
        {
            Debug.LogError("Got " + e + " when trying to TcpClient.GetStream().");
        }
        Reader = new StreamReader(Stream);
        Writer = new StreamWriter(Stream);

        // CurrentState = new StartStreamState(Reader, Writer);
        // Write xml start stream request out and see if we get a response.

        Writer.Write(@"<stream:stream
    xmlns='jabber:client'
    xmlns:stream='http://etherx.jabber.org/streams'
    to='livecoding.tv'
    version='1.0'>");

        Writer.Flush();
        Debug.Log("Wrote stream request out");
    }
    byte[] buffer = new byte[2048];

    private void ReadSecureStream()
    {
        // if (!Input.GetKeyDown(KeyCode.Space)) return;
        // if (ReadSecureStreamThisManyTimes > 5) return; // Unity is freezing if we run this again?
        if (SecureStream == null || !SecureStream.IsAuthenticated) return;
        
        // Read the  message sent by the server.
        // Debug.Log("SecureStream has length " + SecureStream.Length + " and is at position " + SecureStream.Position);

        int bytes = -1;
        if (SecureStream.CanRead && Stream.DataAvailable)
        {
            SecureStream.ReadTimeout = 100;
            bytes = SecureStream.Read(buffer, 0, buffer.Length);
            Decoder decoder = Encoding.UTF8.GetDecoder();
            char[] chars = new char[decoder.GetCharCount(buffer, 0, bytes)];
            // string received = Convert.ToBase64String(buffer);
            decoder.GetChars(buffer, 0, bytes, chars, 0);
            StringBuilder messageData = new StringBuilder();
            messageData.Append(chars);
            if (messageData.Length > 0)
            {
                Debug.Log("Received secure message " + messageData);
                // Debug.Log("Base64 have any effect? " + received);
                ReadSecureStreamThisManyTimes++;
                string messageString = messageData.ToString();
                if (messageString.Contains(@"<mechanism>PLAIN</mechanism>"))
                {
                    RequestPlainSasl();
                }
                if (messageString.Contains(@"<success xmlns='urn:ietf:params:xml:ns:xmpp-sasl'/>"))
                {
                    RequestSaslAuthenticatedStream();
                }
                if (messageString.Contains(@"<bind xmlns='urn:ietf:params:xml:ns:xmpp-bind'/>"))
                {
                    Bind();
                }
                if (messageString.Contains(@"<bind xmlns='urn:ietf:params:xml:ns:xmpp-bind'><jid>"))
                {
                    SessionBind();
                }
                if (messageString.Contains(@"<iq type='result' from='livecoding.tv' id='agsXMPP_2'/>"))
                {
                    JoinRoom();
                }
            }
        }
    }

    private void SessionBind()
    {
        string sessionBindString = @"<iq id=""agsXMPP_2"" type=""set"" to=""livecoding.tv""><session xmlns=""urn:ietf:params:xml:ns:xmpp-session"" /></iq>";
        Debug.Log("Attempting to bind to session " + sessionBindString);
        SendSecure(sessionBindString);
    }

    private void JoinRoom()
    {
        Debug.Log(@"Attempting to join room. <presence to=""10ktactics@chat.livecoding.tv/10kbot"" />");
        SendSecure(@"<presence to=""10ktactics@chat.livecoding.tv/10kbot"" />");
    }

    private void Bind()
    {
        const string bindRequest = @"<iq id=""agsXMPP_1"" type=""set"" to=""livecoding.tv""><bind xmlns=""urn:ietf:params:xml:ns:xmpp-bind"" /></iq>";
        Debug.Log("Sending bind request " + bindRequest);
        SendSecure(bindRequest);
    }

    private void SendSecure(string v)
    {
        byte[] message = Encoding.UTF8.GetBytes(v);
        // byte[] pass = C
        // Set up new readers and writers.
        SecureStream.Write(message);
        SecureStream.Flush();
    }

    private void RequestSaslAuthenticatedStream()
    {
        string requestString = @"<stream:stream to='livecoding.tv' xmlns='jabber:client' xmlns:stream='http://etherx.jabber.org/streams' version='1.0' xml:lang='en'>";
        byte[] message = Encoding.UTF8.GetBytes(requestString);
        // byte[] pass = C
        // Set up new readers and writers.
        Debug.Log("Sending sasl authenticated stream request. binding is next.");
        SecureStream.Write(message);
        SecureStream.Flush();
    }

    private void RequestPlainSasl()
    {
        // Remove old key and change password.
        string requestString = @"<auth xmlns=""urn:ietf:params:xml:ns:xmpp-sasl"" mechanism=""PLAIN"">" + Password + "</auth>";

        byte[] message = Encoding.UTF8.GetBytes(requestString);
        // byte[] pass = C
        // Set up new readers and writers.
        Debug.Log("Sending Plain Sasl request.");
        SecureStream.Write(message);
        SecureStream.Flush();
    }

    int ReadSecureStreamThisManyTimes = 0;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            ReadSecureStream();
        if (ReadSecureStreamThisManyTimes == 0 && (SecureStream == null || !SecureStream.IsAuthenticated) && Stream.DataAvailable)
        {
            // Number of characters that are ready to be read.
            int available = Client.Available;
            char[] buffer = new char[available];
            Reader.ReadBlock(buffer, 0, available);
            string raw = new String(buffer);
            Debug.Log(Time.frameCount + " Received raw: " + raw);
            Parse(raw);
        }
        else if (!Client.Connected)
        {
            Debug.LogWarning(Time.frameCount + " TcpClient is disconnected!");
            enabled = false;
        }
    }

    private void Parse(string raw)
    {
        if (raw.Contains("<starttls"))
        {
            ResponseStartTls();
        }
        if (raw.ToLower().Contains("proceed"))
        {
            PerformTls();
        }
    }

    private void PerformTls()
    {
        // Create an SSL stream that will close the client's stream.
        SecureStream = new SslStream(
            Stream,
            true,
            new RemoteCertificateValidationCallback(ValidateServerCertificate));
        // The server name must match the name on the server certificate.
        try
        {
            SecureStream.AuthenticateAsClient(ServerName);
        }
        catch (AuthenticationException e)
        {
            Debug.LogError("Exception: " + e.Message);
            if (e.InnerException != null)
            {
                Debug.LogError("Inner exception: " + e.InnerException.Message);
            }
            // Console.WriteLine("Authentication failed - closing the connection.");
            Client.Close();
            return;
        }
        // Authenticated!
        string request = @"<stream:stream xmlns='jabber:client' xmlns:stream='http://etherx.jabber.org/streams' to='xmpp.livecoding.tv' version='1.0'>";
        Debug.Log("Asking to open a new XMPP stream on authenticated SecureStream! " + request);
        byte[] message = Encoding.UTF8.GetBytes(request);
        // byte[] message = Convert.FromBase64String(@"<stream:stream xmlns='jabber:client' xmlns:stream='http://etherx.jabber.org/streams' to='xmpp.livecoding.tv' version='1.0'>");

        // Set up new readers and writers.
        SecureStream.Write(message);
        SecureStream.Flush();

        /*
        // Encode a test message into a byte array.
        // Signal the end of the message using the "<EOF>".
        byte[] messsage = Encoding.UTF8.GetBytes("Hello from the client.<EOF>");
        // Send hello message to the server. 
        sslStream.Write(messsage);
        sslStream.Flush();
        // Read message from the server.
        string serverMessage = ReadMessage(sslStream);
        Console.WriteLine("Server says: {0}", serverMessage);
        // Close the client connection.
        client.Close();
        Console.WriteLine("Client closed.");*/
    }

    private bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        Debug.LogWarning("Might want to be a bit more picky with certificates!");
        return true;
    }

    private void ResponseStartTls()
    {
        Debug.Log(Time.frameCount + " Requesting we start tls.");
        Writer.Write(@"<starttls xmlns='urn:ietf:params:xml:ns:xmpp-tls'/>");
        Writer.Flush();
    }

    void OnDisable()
    {
        if (SecureStream != null && SecureStream.IsAuthenticated)
            SecureStream.Close();
        Stream.Close();
        Client.Close();
    }

    private State CurrentState
    {
        get { return MCurrentState; }
        set
        {
            Debug.Log("Changing state from " + MCurrentState + " to " + value);
            MCurrentState = value;
        }
    }
    private State MCurrentState;

    private SslStream SecureStream;
    private NetworkStream Stream;
    private TcpClient Client;
    private StreamReader Reader;
    private StreamWriter Writer;

    private const string ServerName = "xmpp.livecoding.tv";

    private static string Password
    {
        get
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            path += @"\livecodingpasshash.txt";
            string password = File.ReadAllText(path);
            password = password.Trim();
            if (string.IsNullOrEmpty(password))
                throw new Exception("Missing password");
            return password;
        }
    }
}
