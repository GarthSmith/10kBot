using UnityEngine;
// using agsXMPP;
using System.IO;
using System;
// using agsXMPP.Xml.Dom;
// using agsXMPP.protocol.client;
using System.Net;
using Ubiety;
using Ubiety.Infrastructure;
using Ubiety.Common;
using Ubiety.Core;
using Ubiety.Registries;
using System.Xml;
using Ubiety.States;
using System.Net.Sockets;

public class Livecoding : MonoBehaviour
{
    void Awake()
    {
        IPHostEntry ipHostInfo = Dns.GetHostEntry("livecoding.tv");
        IPAddress ipAddress = ipHostInfo.AddressList[0];// IPAddress.Parse(address);
        IPEndPoint endPoint = new IPEndPoint(ipAddress, 5222);

        LogQueue.Log("Attempting to connect socket.");

        var _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        try {
            _socket.Connect(endPoint);
            LogQueue.Log("Connected!");
        }
        catch (Exception e)
        {
            LogQueue.Error("Unable to connect got " + e + ": " + e.Message);
        }

        LivecodingXmpp = new Xmpp();
    }

    void Initialize()
    {
        if (Initialized) return;
        Xmpp.Settings.Id = new JID("10kbot@livecoding.tv");
        Xmpp.Settings.Password = PasswordHash;
        Xmpp.Settings.AuthenticationTypes = MechanismType.Plain;
        Xmpp.Settings.Ssl = true;

        ProtocolState.Settings.Id = new JID("10kbot@livecoding.tv");
        ProtocolState.Settings.Password = PasswordHash;
        ProtocolState.Settings.Ssl = true;
        ProtocolState.Settings.AuthenticationTypes = MechanismType.Plain;

        LivecodingXmpp.OnError += HandleError;
        LivecodingXmpp.OnNewTag += HandleNewTag;
        LivecodingXmpp.Connect();
        Initialized = true;
    }
    bool Initialized;

    void Update()
    {
        // Initialize();

        ConsoleXmpp.Connect();
        enabled = false;
        if (Time.time < 5f)// Wait a few seconds.
        {
            return;
        }
       //  enabled = false;

        /*if (ProtocolState.State is SessionState)
        {
            // Create join room request.
            GenericTag joinTag = new GenericTag(new XmlQualifiedName("presence"));
            joinTag.SetAttribute("from", "10kbot@livecoding.tv/10kbot");
            joinTag.SetAttribute("id", "ng91xs69"); // Needed?
            //joinTag.SetAttribute("to", "10ktactics@chat.livecoding.tv/10ktactics");
            joinTag.SetAttribute("to", "10ktactics@chat.livecoding.tv/10kbot");

            // Send it.
            LogQueue.Log(Time.frameCount + " Attempting to send tag " + joinTag);
            LivecodingXmpp.Send(joinTag);
        }*/
    }

    private void HandleNewTag(object sender, TagEventArgs e)
    {
        /*if (e != null)
            LogQueue.Log("Received new tag from " + sender);
        else
            LogQueue.Log("Received new tag " + e.Tag + " from " + sender);*/
    }

    private void HandleError(object sender, Ubiety.Infrastructure.ErrorEventArgs e)
    {
        if (e != null)
            LogQueue.Error("Received error " + e.Type + ": " + e.Message + " from " + sender);
        else
            LogQueue.Error("Received error from " + sender);
    }

    public void Connect()
    {
        LogQueue.Log("Attempting to connect to chat.livecoding.tv");
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

    private static string Password
    {
        get
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            path += @"\livecodingpass.txt";
            string password = File.ReadAllText(path);
            password = password.Trim();
            return password;
        }
    }

    private Xmpp LivecodingXmpp;
}
