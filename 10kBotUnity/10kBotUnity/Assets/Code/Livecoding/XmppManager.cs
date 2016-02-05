/* Garth: Writing this so all the connection and room join code can
 * be easily used in Unity without relying on static void Main().
 */

using agsXMPP;
using agsXMPP.protocol.client;
using agsXMPP.protocol.iq.disco;
using agsXMPP.Xml.Dom;
using System;
using System.IO;
using UnityEngine;
using System.Threading;
using System.Net.Sockets;
using agsXMPP.net;

public class XmppManager
{
    static bool _bWait;
    private static Jid chatjid;
    private static XmppClientConnection conn;
    private Jid jid;

    public ClientSocket ProblemSocket {  get { return conn == null ? null : conn.ProblemSocket; } }


    public void Go()
    {
        Debug.Log("XmppManager is Go()ing!");
        //set the default chatroom we want to join 
        chatjid = new Jid("10ktactics@chat.livecoding.tv");
        //create a new client connection
        conn = new XmppClientConnection();
        //add handlers for various shits 
        // some of these handlers are events that trigger other events. onLogin triggers disco
        conn.OnLogin += conn_OnLogin;
        conn.OnMessage += conn_OnMessage;
        conn.OnError += conn_OnError;
        conn.OnBinded += conn_OnBinded;
        conn.OnRosterStart += conn_OnRosterStart;
        conn.OnRosterEnd += conn_OnRosterEnd;
        conn.OnAuthError += conn_OnAuthError;
        conn.OnClose += conn_OnClose;
        conn.OnRegisterError += conn_OnRegisterError;
        conn.OnSocketError += conn_OnSocketError;
        conn.OnStreamError += conn_OnStreamError;
        conn.OnXmppConnectionStateChanged += conn_OnXmppConnectionStateChanged;

        // LogQueue.Log("XmppManager is Connect()ing!");
        jid = new Jid("10kbot@livecoding.tv");
        conn.Server = jid.Server;
        conn.Username = jid.User;
        conn.Password = Password;
        conn.Resource = null;
        conn.Priority = 10;
        conn.Port = 5222;
        conn.UseSSL = false;
        conn.AutoResolveConnectServer = true;
        conn.UseStartTLS = true;
        Debug.Log("Ready to connect!");
    }

    private static void conn_OnXmppConnectionStateChanged(object sender, XmppConnectionState state)
    {
        PrintEvent("XmppConnectionStateChanged to " + state);
    }

    private static void conn_OnStreamError(object sender, Element e)
    {
        PrintEvent("Got conn_OnStreamError");
    }

    private static void conn_OnSocketError(object sender, Exception ex)
    {
        PrintEvent("Got conn_OnSocketError");
    }

    private static void conn_OnRegisterError(object sender, Element e)
    {
        PrintEvent("Got Register Error");
    }

    private static void conn_OnClose(object sender)
    {
        PrintEvent("Closed");
    }

    private static void conn_OnAuthError(object sender, Element e)
    {
        PrintEvent("Got Auth Error");
    }

    private static void conn_OnRosterEnd(object sender)
    {
        PrintEvent("Got roster!");
        JoinRoom();
    }

    private static void conn_OnRosterStart(object sender)
    {
        PrintEvent("Getting roster");
    }

    private static void conn_OnBinded(object sender)
    {
        PrintEvent("Bound.");
    }

    private static void conn_OnError(object sender, Exception ex)
    {
        PrintEvent("Receieved " + ex + " from " + sender + ". " + ex.Message);
    }

    public static void Connect()
    {
        Debug.Log("XmppManager is Connect()ing!");
        /*Jid jid = new Jid("10kbot@livecoding.tv");
        conn.Server = jid.Server;
        conn.Username = jid.User;
        conn.Password = Password;
        conn.Resource = null;
        conn.Priority = 10;
        conn.Port = 5222;
        conn.UseSSL = false;
        conn.AutoResolveConnectServer = true;
        conn.UseStartTLS = true;*/
        conn.Open();
        Debug.Log("Connection attempted/opened");
        // Wait("Waiting 2 seconds for response.");
        //return if disconnected
        // if (conn.XmppConnectionState == XmppConnectionState.Disconnected)
        //     return;
        // PrintInfo("Connection state is now " + conn.XmppConnectionState);
    }

    private static void conn_OnMessage(object sender, Message msg)
    {
        PrintEvent(msg.From + ": " + msg.Body);
    }

    private static void conn_OnLogin(object sender)
    {
        PrintEvent("SUP on login!");
    }

    //disco result 
    private static void onDisco(object sender, IQ iq, object data)
    {
        if (iq.Type == IqType.result)
        {
            Element e = iq.Query;
            if (e != null && e.GetType() == typeof(DiscoItems))
            {
                DiscoItems di = e as DiscoItems;
                DiscoItem[] ditems = di.GetDiscoItems();
                DiscoManager dm = new DiscoManager(conn);
                foreach (DiscoItem i in ditems)
                {
                    //gets a list of items chat.livecoding.tv pubsub.livecoding.tv and vjud.livecoding.tv so lets get info for each item
                    //Console.WriteLine(i.ToString());
                    dm.DiscoverInformation(i.Jid, new IqCB(onDiscoInfo), i);
                }
            }
        }
    }

    private static void onDiscoInfo(object sender, IQ iq, object data)
    {
        PrintInfo(iq.ToString());
    }

    /// <summary>
    /// Join the 10ktactics chat room
    /// </summary>
    private static void JoinRoom()
    {
        Presence pres = new Presence();
        chatjid.Resource = "10kbot"; //your bot's name
        pres.To = chatjid;
        conn.Send(pres);
    }

    static void PrintEvent(string msg)
    {
#if UNITY_5
        UnityEngine.Debug.Log(msg);
#else
            ConsoleColor current = Console.BackgroundColor;
            Console.BackgroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine(msg);
            Console.BackgroundColor = current;
#endif
    }

    static void PrintInfo(string msg)
    {
#if UNITY_5
        UnityEngine.Debug.Log(msg);
#else
            ConsoleColor current = Console.BackgroundColor;
            Console.BackgroundColor = ConsoleColor.Red;
            Console.WriteLine(msg);
            Console.BackgroundColor = current;
#endif
    }

    static void PrintHelp(string msg)
    {
#if UNITY_5
        UnityEngine.Debug.Log(msg);
#else
            ConsoleColor current = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(msg);
            Console.ForegroundColor = current;
#endif
    }

    private static string Password
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
