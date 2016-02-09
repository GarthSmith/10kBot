/*using System;
using System.IO;
using System.Net.Sockets;
using System.Collections;
using UnityEngine;
using System.Text.RegularExpressions;
using MTB;

public class Twitch : MonoBehaviour, IChat
{
    public event Action<string, string> MessageReceived;
    public Action<string> Message;

    public const string Server = "irc.twitch.tv";
    public const int Port = 6667;

    public const string TwitchUser = "10kbot";
    public const string TwitchChannel = "#10ktactics"; // # because it's an IRC channel.

    public void Login()
    {
        Networking.Twitch = this;
        Networking.Connect();
        // TcpClient.Connect("irc.twitch.tv", 6667);
        // NetworkStream = TcpClient.GetStream();

        Networking.AddStaticCommand("USER " + TwitchUser + "tmi twitch :" + TwitchUser);
        Networking.AddStaticCommand("PASS " + TwitchPassword);
        Networking.AddStaticCommand("NICK " + TwitchUser);

        // Don't need to see who is entering and leaving the channel.
        // See: https://github.com/justintv/Twitch-API/blob/master/IRC.md
        // Networking.AddStaticCommand("CAP REQ :twitch.tv/membership");


        Networking.AddStaticCommand("JOIN " + TwitchChannel);
        Networking.Flush();
    }

    internal void LogOut()
    {
        // Networking.Disconnect();
    }

    public static string TwitchPassword
    {
        get
        {
            string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
            path += @"\twitchoauthpassword.txt";
            string password = File.ReadAllText(path);
            password = password.Trim();
            return password;
        }
    }

    Regex someExpression = new Regex(@"(?::(?<Prefix>[^ ]+) +)?(?<Command>[^ :]+)(?<middle>(?: +[^ :]+))*(?<coda> +:(?<trailing>.*)?)?");

    private void ParseData(string data)
    {
        Match someMatch = someExpression.Match(data);

        string prefix = someMatch.Groups["Prefix"].Value;
        string command = someMatch.Groups["Command"].Value;

        // split the data into parts
        string[] ircData = data.Split(' ');

        // if the message starts with PING we must PONG back
        if (data.Length > 4)
        {
            if (data.Substring(0, 4) == "PING")
            {
                Networking.AddStaticCommand("PONG " + ircData[1]);
                return;
            }
        }

        // re-act according to the IRC Commands -- STRIPPED DOWN VERSION -- ADD YOUR OWN
        switch (command)
        {
            case "PRIVMSG": // message was sent to the channel or as private
                            // if it's a private message
                if (ircData[2].ToLower() != TwitchUser.ToLower())
                {
                    OnChannelMessage(new ChannelMessageEventArgs(ircData[2], ircData[0].Substring(1, ircData[0].IndexOf('!') - 1), JoinArray(ircData, 3)));
                }
                break;
        }
    }

    //Joins the array into a string after a specific index
    private string JoinArray(string[] strArray, int startIndex)
    {
        return StripMessage(String.Join(" ", strArray, startIndex, strArray.Length - startIndex));
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

    private void OnChannelMessage(ChannelMessageEventArgs channelMessageEventArgs)
    {
        if (Message != null && channelMessageEventArgs != null)
            Message(channelMessageEventArgs.From + ": " + channelMessageEventArgs.Message);
    }
}
*/