/* https://dev.twitch.tv/docs/irc/
 */

using System;
using System.Collections;
using System.IO;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using UnityEngine;

#region delegates

public delegate void ChannelMessage(ChannelMessageEventArgs channelMessageArgs);

#endregion

public class Twitch : MonoBehaviour, IChat
{
    /// <summary>
    /// Announce we received a message from Twitch.
    /// Provides Username and Msg.
    /// </summary>
    public event Action<string, string> MessageReceived;

    #region variables
    // public Text ChannelUiText;

    private const string ServerName = "irc.twitch.tv";
    private const int ServerPort = 6667;

    public static Twitch Instance;

    public bool ConnectOnStart = true;

    public string Username = "garthbot";

    private string OauthToken => TwitchPassword;

    public string Channel = "#originalgarth";

    private TcpClient ircTcpClient;

    private NetworkStream stream;

    private string inputLine;

    private StreamReader reader;

    private StreamWriter writer;

    #endregion

    #region public_methods

    /// <summary>
    /// Connect to Twitch IRC server
    /// </summary>
    private void Connect()
    {
        if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(OauthToken))
        {
            Debug.LogError($"{GetType()}.{nameof(Connect)} missing {nameof(Username)} or {nameof(OauthToken)}");
            return;
        }

        try
        {
            ircTcpClient = new TcpClient(ServerName, ServerPort);
            stream = ircTcpClient.GetStream();
            reader = new StreamReader(stream);
            writer = new StreamWriter(stream);

            Send("USER " + Username + "tmi twitch :" + Username);
            Send("PASS " + OauthToken);
            Send("NICK " + Username);

            StartCoroutine(Listen());
        }
        catch (Exception ex)
        {
            Debug.LogError("Recieved error connecting to twitch. got " + ex.GetType());
        }
    }

    /// <summary>
    /// Disconnect from Twitch IRC server
    /// </summary>
    public void Disconnect()
    {
        ircTcpClient = null;
        StopCoroutine("Listen");

        if (stream != null)
            stream.Dispose();
        if (writer != null)
            writer.Dispose();
        if (reader != null)
            reader.Dispose();
    }

    #endregion

    #region private_methods

    private IEnumerator Listen()
    {
        while (true)
        {
            if (!ircTcpClient.Connected)
                Debug.LogError("Lost connection!");

            if (stream.DataAvailable)
            {
                var available = ircTcpClient.Available;
                var buffer = new char[available];
                reader.ReadBlock(buffer, 0, ircTcpClient.Available);
                var raw = new string(buffer);
                Debug.Log("Received raw:");
                Debug.Log(raw);
                raw = raw.Replace("\r", "");
                foreach (var split in raw.Split('\n'))
                {
                    ParseData(split);
                }
            }

            yield return null;
        }
    }

    private void ParseData(string data)
    {
        if (string.IsNullOrEmpty(data)) return;

        Debug.Log("Parsing: " + data);
        // split the data into parts
        var ircData = data.Split(' ');

        // if the message starts with PING we must PONG back
        if (data.Length > 4)
        {
            if (data.Substring(0, 4) == "PING")
            {
                Send("PONG " + ircData[1]);
                Debug.Log("PONGed!");
                return;
            }
        }

        // re-act according to the IRC Commands -- STRIPPED DOWN VERSION -- ADD YOUR OWN
        Debug.Log("Received from twitch command " + ircData[1] + ", will print if PRIVMSG");
        switch (ircData[1])
        {
            case "001": // server welcome message, after this we can join
                Send("MODE " + Username + " +B");
                OnConnectedToServer();
                break;

            case "PRIVMSG": // message was sent to the channel or as private
                // if it's a private message
                Debug.Log("Got a PRIVMSG!");
                if (ircData[2].ToLower() != Username.ToLower())
                { 
                    OnChannelMessage(new ChannelMessageEventArgs(ircData[2], ircData[0].Substring(1, ircData[0].IndexOf('!') - 1), JoinArray(ircData, 3)));
                }
                break;

            default:
                Debug.Log("Got " + ircData[1] + ", raw response: " + data);
                break;
        }
    }

    

    private void OnChannelMessage(ChannelMessageEventArgs channelMessageEventArgs)
    {
        // Announce
        if (MessageReceived != null)
        {
            MessageReceived(channelMessageEventArgs.From, channelMessageEventArgs.Message);
        }
    }

    //Strips the message of unnecessary characters
    private string StripMessage(string message)
    {
        // remove IRC Color Codes
        foreach (Match m in new Regex((char)3 + @"(?:\d{1,2}(?:,\d{1,2})?)?").Matches(message))
            message = message.Replace(m.Value, "");

        // if there is nothing to strip
        if (message == "")
            return "";
        if (message.Substring(0, 1) == ":" && message.Length > 2)
            return message.Substring(1, message.Length - 1);
        return message;
    }

    //Joins the array into a string after a specific index
    private string JoinArray(string[] strArray, int startIndex)
    {
        return StripMessage(string.Join(" ", strArray, startIndex, strArray.Length - startIndex));
    }

    private void Send(string message)
    {
        Debug.Log("Sending twitch " + message);
        writer.WriteLine(message);
        writer.Flush();
    }

    private void Message(string message)
    {
        Send("PRIVMSG " + Channel + " " + message);
    }

    private void OnConnectedToServer()
    {
        Send("JOIN " + Channel);
    }

    #endregion

    private void Start()
    {
        Instance = this;

        if (ConnectOnStart)
            Connect();
    }

    private void OnDisable()
    {
        Disconnect();
    }

    public static string TwitchPassword
    {
        get
        {
            var path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
            path += @"\twitchoauthpassword.txt";
            if (!File.Exists(path))
            {
                Debug.LogError($"Unable to find secret at {path}");
                return null;
            }

            var password = File.ReadAllText(path);
            password = password.Trim();
            return password;
        }
    }
}