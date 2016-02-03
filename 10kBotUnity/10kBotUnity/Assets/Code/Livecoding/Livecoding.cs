using UnityEngine;
using agsXMPP;
using System.IO;
using System;
using agsXMPP.Xml.Dom;
using agsXMPP.protocol.client;
using System.Net;

public class Livecoding : MonoBehaviour {

    void Start()
    {
        Bind();
        // Connect();

        // Test looking up host.
        try {
            IPHostEntry hostEntry = Dns.GetHostEntry("livecoding.tv");
            Debug.Log("Got hostEntry " + hostEntry.HostName + " and " + hostEntry.AddressList.Length + " ip addresses.");

        }
        catch (Exception e)
        {
            Debug.LogError("Unable to find chat.livecoding.tv. Got " + e + ": " + e.Message);
        }
    }

    void Bind()
    {
        xmppClient.OnAuthError += HandleAuthError;
        xmppClient.OnError += HandleError;
        xmppClient.OnLogin += HandleLogin;
        xmppClient.OnPresence += HandlePresense;
        xmppClient.OnSocketError += HandleSocketError;
        xmppClient.OnStreamError += HandleStreamError;
        xmppClient.OnXmppConnectionStateChanged += HandleXmppConnectionStateChanged;
        xmppClient.OnMessage += HandleMessage;
    }

    private void HandleMessage(object sender, Message msg)
    {
        Debug.Log("Yay! Getting messages! Figure out which ones are from the chat.");
    }

    private void HandleAuthError(object sender, Element e)
    {
        Debug.Log("HandleAuthError got sender type " + sender + ", e: " + e);
    }

    private void HandleError(object sender, Exception ex)
    {
        Debug.Log("HandleError got sender type " + sender + ", ex: " + ex);
    }

    private void HandleLogin(object sender)
    {
        Debug.Log("HandleLogin got sender " + sender);
    }

    private void HandlePresense(object sender, Presence pres)
    {
        Debug.Log("HandlePresense got sender " + sender + " and pres " + pres);
    }

    private void HandleSocketError(object sender, Exception ex)
    {
        Debug.Log("HandleSocketError got sender type " + sender + ", ex: " + ex);
    }

    private void HandleStreamError(object sender, Element e)
    {
         Debug.Log("HandleAuthError got sender type " + sender + ", e: " + e);
    }

    private void HandleXmppConnectionStateChanged(object sender, XmppConnectionState state)
    {
        Debug.Log(sender + " is announcing connection state is now " + state);
    }

    public void Connect()
    {
        Debug.Log("Attempting to connect to chat.livecoding.tv");
        xmppClient.Open("10ktactics@livecoding.tv", Password);
        
    }

    public static string Password
    {
        get
        {
            string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
            path += @"\livecodingpass.txt";
            string password = File.ReadAllText(path);
            password = password.Trim();
            return password;
        }
    }

    XmppClientConnection xmppClient = new XmppClientConnection("chat.livecoding.tv");
}
