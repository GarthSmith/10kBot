/* Written by Garth Smith.
 * Free to use by anyone under whatever GPL, MIT, BSD, CC license they want.
 */

using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Collections;
using System.Text;
using UnityEngine;
using System.Xml;

/// <summary>
/// livecoding.tv is a now defunct site that was like a Twitch for programming. Keeping this file in case we want to
/// connect to similar chat services. 
/// </summary>
public class Livecoding : MonoBehaviour, IChat
{
    public event Action<string, string> MessageReceived;

    private void Awake()
    {
        Client = new TcpClient(ServerName, 5222);
    }

    private void OnEnable()
    {
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

        // Write xml start stream request out and see if we get a response.

        Writer.Write(@"<stream:stream
    xmlns='jabber:client'
    xmlns:stream='http://etherx.jabber.org/streams'
    to='livecoding.tv'
    version='1.0'>");

        Writer.Flush();
        StartCoroutine(PollingRoutine());
        Debug.Log("Wrote stream request out");
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        if (SecureStream != null && SecureStream.IsAuthenticated)
            SecureStream.Close();
        Stream.Close();
        Client.Close();
    }

    private IEnumerator PollingRoutine()
    {
        while (enabled)
        {
            yield return new WaitForSeconds(2.5f);
            ReadSecureStream();
        }
    }

    private byte[] buffer = new byte[2048];
    
    private void ReadSecureStream()
    {
        // BUG: Unity is freezing if we run this too often!
        // Probably need to use some kind of lock on the stream.
        if (SecureStream == null || !SecureStream.IsAuthenticated) return;

        // Read the  message sent by the server.
        var bytes = -1;
        if (SecureStream.CanRead && Stream.DataAvailable)
        {
            SecureStream.ReadTimeout = 100;
            bytes = SecureStream.Read(buffer, 0, buffer.Length);
            var decoder = Encoding.UTF8.GetDecoder();
            var chars = new char[decoder.GetCharCount(buffer, 0, bytes)];
            // string received = Convert.ToBase64String(buffer);
            decoder.GetChars(buffer, 0, bytes, chars, 0);
            var messageData = new StringBuilder();
            messageData.Append(chars);
            if (messageData.Length > 0)
            {
                Debug.Log("Received secure message " + messageData);
                // Debug.Log("Base64 have any effect? " + received);
                ReadSecureStreamThisManyTimes++;
                var messageString = messageData.ToString();
                if (messageString.Contains(@"<mechanism>PLAIN</mechanism>"))
                {
                    RequestPlainSasl();
                }
                else if (messageString.Contains(@"<success xmlns='urn:ietf:params:xml:ns:xmpp-sasl'/>"))
                {
                    RequestSaslAuthenticatedStream();
                }
                else if (messageString.Contains(@"<bind xmlns='urn:ietf:params:xml:ns:xmpp-bind'/>"))
                {
                    Bind();
                }
                else if (messageString.Contains(@"<bind xmlns='urn:ietf:params:xml:ns:xmpp-bind'><jid>"))
                {
                    SessionBind();
                }
                else if (messageString.Contains(@"<iq type='result' from='livecoding.tv' id='agsXMPP_2'/>"))
                {
                    JoinRoom();
                }
                else
                {
                    LookForMessages(messageString);
                }
            }
        }
    }

    /// <summary>
    /// Picks messages out of incoming XML.
    /// </summary>
    private void LookForMessages(string messageString)
    {
        if (CumulativeXml == null)
            CumulativeXml = "";
        CumulativeXml += messageString;

        // Messages are Xml elements
        // wrapped in <message ... />
        // Eg:
        // <message from='10ktactics@chat.livecoding.tv/xmetrix' to='10kbot@livecoding.tv/27180579011454665231434439' type='groupchat' id='149'><body xmlns='jabber:client'>bet its a pitbull</body><x xmlns='jabber:x:event'><composing/></x><delay xmlns='urn:xmpp:delay' from='10ktactics@chat.livecoding.tv' stamp='2016-02-05T07:50:55.210Z'/><x xmlns='jabber:x:delay' from='10ktactics@chat.livecoding.tv' stamp='20160205T07:50:55'/></message>

        while (CumulativeXml.Contains(StartMessageToken))
        {
            // Start to process. We don't care about anything before the message.
            CumulativeXml = TrimFront(CumulativeXml);
            var singleMessageXml = GetMessageXml(CumulativeXml);
            ProcessXml(singleMessageXml);
            CumulativeXml = CumulativeXml.Substring(singleMessageXml.Length - 1);
        }
    }

    /// <summary>
    /// Hopefully a well formed xml string.
    /// </summary>
    private void ProcessXml(string singleMessageXml)
    {
        if (string.IsNullOrEmpty(singleMessageXml))
            return;
        if (!singleMessageXml.StartsWith(StartMessageToken) || !singleMessageXml.EndsWith(EndMessageToken))
            return;
        Debug.Log("About to parse singlemessagexml\n" + singleMessageXml);
        var messageDoc = new XmlDocument();
        messageDoc.LoadXml(singleMessageXml);
        Debug.Log("We got xml doc " + messageDoc);

        // Eg:
        // <message from='10ktactics@chat.livecoding.tv/xmetrix'
        //      to ='10kbot@livecoding.tv/27180579011454665231434439'
        //      type ='groupchat' id='149'>
        //      <body xmlns='jabber:client'>bet its a pitbull</body>
        //      <x xmlns='jabber:x:event'><composing/></x><delay xmlns='urn:xmpp:delay' from='10ktactics@chat.livecoding.tv' stamp='2016-02-05T07:50:55.210Z'/><x xmlns='jabber:x:delay' from='10ktactics@chat.livecoding.tv' stamp='20160205T07:50:55'/></message>

        // Find these two string.
        var nickname = "";
        var message = "";

        // XmlNamespaceManager nsmgr = new XmlNamespaceManager(messageDoc.NameTable);
        // nsmgr.AddNamespace("ab", "http://www.lucernepublishing.com");
        var messageNode = messageDoc.SelectSingleNode(".//message");
        
        if (messageNode == null || messageNode.Attributes == null)
        {
            Debug.Log("Yeah attributes is null. " + (messageNode == null ? "null" : messageNode.OuterXml));
        }
        else {
            var fromAttribute = messageNode.Attributes["from"];
            if (fromAttribute != null)
            {
                nickname = fromAttribute.Value;
                nickname = nickname.Replace("10ktactics@chat.livecoding.tv/", "");
            }
            else
            {
                Debug.Log("How are we missing the from attribute from message doc? " + messageDoc.OuterXml);
            }
        }

        // TODO: How to get body child element from messageDoc.
        message = messageNode.InnerXml;
        Debug.Log("Got messageNode.InnerXml: " + message);
        // Chop <body ...> part of beginning of message.
        message = message.Substring(StartMessageBodyToken.Length);
        // Chop </body>... off the end
        message = message.Substring(0, message.IndexOf(EndMessageBodyToken));

        Debug.Log("Got nickname " + nickname + " and message " + message);
        if (MessageReceived != null && !string.IsNullOrEmpty(nickname) && !string.IsNullOrEmpty(message))
            MessageReceived(nickname, message);
    }

    private string GetMessageXml(string cumulativeXml)
    {
        var endIndex = cumulativeXml.IndexOf(EndMessageToken) + EndMessageToken.Length;
        var message = cumulativeXml.Substring(0, endIndex);
        Debug.LogWarning("Got message xml " + message);
        return message;
    }

    /// <summary>
    /// Remove everything before the first <message xml.
    /// </summary>
    private string TrimFront(string trimThis)
    {
        if (string.IsNullOrEmpty(trimThis))
            return trimThis;
        var startIndex = trimThis.IndexOf(StartMessageToken);
        if (startIndex < 0) // not found
            return trimThis;
        var trimmed = trimThis.Substring(startIndex);
        Debug.Log("Trimmed front to " + trimmed.Substring(0, 10) + "...");
        return trimmed;
    }

    private string CumulativeXml;

    private void SessionBind()
    {
        var sessionBindString = @"<iq id=""agsXMPP_2"" type=""set"" to=""livecoding.tv""><session xmlns=""urn:ietf:params:xml:ns:xmpp-session"" /></iq>";
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
        var message = Encoding.UTF8.GetBytes(v);
        // byte[] pass = C
        // Set up new readers and writers.
        SecureStream.Write(message);
        SecureStream.Flush();
    }

    private void RequestSaslAuthenticatedStream()
    {
        var requestString = @"<stream:stream to='livecoding.tv' xmlns='jabber:client' xmlns:stream='http://etherx.jabber.org/streams' version='1.0' xml:lang='en'>";
        var message = Encoding.UTF8.GetBytes(requestString);
        // byte[] pass = C
        // Set up new readers and writers.
        Debug.Log("Sending sasl authenticated stream request. binding is next.");
        SecureStream.Write(message);
        SecureStream.Flush();
    }

    private void RequestPlainSasl()
    {
        // Remove old key and change password.
        var requestString = @"<auth xmlns=""urn:ietf:params:xml:ns:xmpp-sasl"" mechanism=""PLAIN"">" + Password + "</auth>";

        var message = Encoding.UTF8.GetBytes(requestString);
        // byte[] pass = C
        // Set up new readers and writers.
        Debug.Log("Sending Plain Sasl request.");
        SecureStream.Write(message);
        SecureStream.Flush();
    }

    private int ReadSecureStreamThisManyTimes = 0;

    private void Update()
    {
        if (ReadSecureStreamThisManyTimes == 0 && (SecureStream == null || !SecureStream.IsAuthenticated) && Stream.DataAvailable)
        {
            // Number of characters that are ready to be read.
            var available = Client.Available;
            var buffer = new char[available];
            Reader.ReadBlock(buffer, 0, available);
            var raw = new string(buffer);
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
        var request = @"<stream:stream xmlns='jabber:client' xmlns:stream='http://etherx.jabber.org/streams' to='xmpp.livecoding.tv' version='1.0'>";
        Debug.Log("Asking to open a new XMPP stream on authenticated SecureStream! " + request);
        var message = Encoding.UTF8.GetBytes(request);
        // byte[] message = Convert.FromBase64String(@"<stream:stream xmlns='jabber:client' xmlns:stream='http://etherx.jabber.org/streams' to='xmpp.livecoding.tv' version='1.0'>");

        // Set up new readers and writers.
        SecureStream.Write(message);
        SecureStream.Flush();
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
            var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            path += @"\livecodingpasshash.txt";
            var password = File.ReadAllText(path);
            password = password.Trim();
            if (string.IsNullOrEmpty(password))
                throw new Exception("Missing password");
            return password;
        }
    }

    private const string StartMessageToken = @"<message ";
    private const string EndMessageToken = @"</message>";
    private const string StartMessageBodyToken = @"<body xmlns='jabber:client'>";
    private const string EndMessageBodyToken = @"</body>";
}
