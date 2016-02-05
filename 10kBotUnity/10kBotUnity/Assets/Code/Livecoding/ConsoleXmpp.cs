using System;
using System.Text;
using System.Threading;

using agsXMPP;
using agsXMPP.protocol;
using agsXMPP.protocol.client;
using System.IO;

public class ConsoleXmpp
{
    public static XmppClientConnection xmppCon { get; private set; }

    static bool _bWait;

    public static void Disconnect()
    {
        xmppCon.Close();
        xmppCon = null;
    }

    /// <summary>
    /// Attempt to connect.
    /// </summary>
    public static void Connect()
    {
        LogQueue.Log("Connecting");
        xmppCon = new XmppClientConnection();

        // Console.Title = "Console Client";

        // read the jid from the console
        // PrintHelp("Enter you Jid (user@server.com): ");
        Jid jid = new Jid("10kbot@livecoding.tv");

        // PrintHelp(String.Format("Enter password for '{0}': ", jid.ToString()));

        xmppCon.Password = PasswordHash;
        xmppCon.Username = jid.User;
        xmppCon.Server = jid.Server;
        xmppCon.AutoAgents = false;
        xmppCon.AutoPresence = true;
        xmppCon.AutoRoster = true;
        xmppCon.AutoResolveConnectServer = true;

        // Connect to the server now 
        // !!! this is asynchronous !!!
        try
        {
            xmppCon.OnRosterStart += new ObjectHandler(xmppCon_OnRosterStart);
            xmppCon.OnRosterItem += new XmppClientConnection.RosterHandler(xmppCon_OnRosterItem);
            xmppCon.OnRosterEnd += new ObjectHandler(xmppCon_OnRosterEnd);
            xmppCon.OnPresence += new PresenceHandler(xmppCon_OnPresence);
            xmppCon.OnMessage += new MessageHandler(xmppCon_OnMessage);
            xmppCon.OnLogin += new ObjectHandler(xmppCon_OnLogin);

            LogQueue.Log("Opening connection.");
            xmppCon.Open();
            LogQueue.Log("Done attempting to open connection.");

        }
        catch (Exception e)
        {
            LogQueue.Error("Got " + e + " when trying to connect!");
            // Console.WriteLine(e.Message);
        }

        // Wait("Login to server, please wait");

        // PrintCommands();

        /*bool bQuit = false;

        while (!bQuit)
        {
            string command = Console.ReadLine();
            string[] commands = command.Split(' ');

            switch (commands[0].ToLower())
            {
                case "help":
                    PrintCommands();
                    break;
                case "quit":
                    bQuit = true;
                    break;
                case "msg":
                    string msg = command.Substring(command.IndexOf(commands[2]));
                    xmppCon.Send(new Message(new Jid(commands[1]), MessageType.chat, msg));
                    break;
                case "status":
                    switch (commands[1])
                    {
                        case "online":
                            xmppCon.Show = ShowType.NONE;
                            break;
                        case "away":
                            xmppCon.Show = ShowType.away;
                            break;
                        case "xa":
                            xmppCon.Show = ShowType.xa;
                            break;
                        case "chat":
                            xmppCon.Show = ShowType.chat;
                            break;
                    }
                    string status = command.Substring(command.IndexOf(commands[2]));
                    xmppCon.Status = status;
                    xmppCon.SendMyPresence();
                    break;
            }
        }

        // close connection
        xmppCon.Close();
        */
    }

    static void xmppCon_OnLogin(object sender)
    {
        // Console.WriteLine();
        LogQueue.Log("Logged in to server");
    }

    static void xmppCon_OnRosterEnd(object sender)
    {
        _bWait = false;
        // Console.WriteLine();
        LogQueue.Log("All contacts received");
        Presence pres = new Presence();

        var chatJid = new Jid("10ktactics@chat.livecoding.tv/10kbot"); // Chat
                                                                       // chatJid.Resource = new Jid("10kbot@livecoding.tv/10kbot"); // mine
        pres.To = chatJid;
        xmppCon.Send(pres);
    }

    static void xmppCon_OnRosterItem(object sender, agsXMPP.protocol.iq.roster.RosterItem item)
    {
        LogQueue.Log(String.Format("Got contact: {0}", item.Jid));
    }

    static void xmppCon_OnRosterStart(object sender)
    {
        LogQueue.Log("Getting contacts now");
    }

    static void xmppCon_OnPresence(object sender, Presence pres)
    {
        LogQueue.Log(String.Format("Got presence from: {0}", pres.From.ToString()));
        LogQueue.Log(String.Format("type: {0}", pres.Type.ToString()));
        LogQueue.Log(String.Format("status: {0}", pres.Status));
        LogQueue.Log("");
    }

    static void xmppCon_OnMessage(object sender, Message msg)
    {
        LogQueue.Log("OnMessage");
        if (msg.Body != null)
        {
            LogQueue.Log(String.Format("Got message from: {0}", msg.From.ToString()));
            LogQueue.Log("message: " + msg.Body);
            LogQueue.Log("");
        }
    }

    private static string PasswordHash
    {
        get
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            path += @"\livecodingpasshash.txt";
            string password = File.ReadAllText(path);
            password = password.Trim();
            return password;
        }
    }
}
