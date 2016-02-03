/*using System;
using System.IO;
using System.Net.Sockets;

public class TwitchFromScratch : MonoBehaviour {
    private TcpClient irc;
    private object stream;
    private StreamReader reader;
    private StreamWriter writer;

    public event Action<string> Received;

    public void Connect()
    {
        if (String.IsNullOrEmpty(Username) || String.IsNullOrEmpty(OauthToken))
            return;

        try
        {
            irc = new TcpClient(ServerName, ServerPort);
            stream = irc.GetStream();
            reader = new StreamReader(stream);
            writer = new StreamWriter(stream);

            Send("USER " + Username + "tmi twitch :" + Username);
            Send("PASS " + Twitch.TwitchPassword);
            Send("NICK " + Username);
            Send("JOIN " + Twitch.TwitchChannel);

            StartCoroutine("Listen");
        }
        catch (Exception ex)
        {

        }
    }

    private void Send(string v)
    {
        throw new NotImplementedException();
    }

    private string Username { get { return Twitch.TwitchUser; } }
    private string 
}
*/