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

public class Livecoding : MonoBehaviour
{
    void Awake()
    {
        LivecodingXmpp = new Xmpp();
    }

    void Initialize()
    {
        if (Initialized) return;
        Xmpp.Settings.Id = new JID("10kbot@livecoding.tv");
        Xmpp.Settings.Password = PasswordHash;
        ProtocolState.Settings.Ssl = true;

        LivecodingXmpp.OnError += HandleError;
        LivecodingXmpp.OnNewTag += HandleNewTag;
        LivecodingXmpp.Connect();
        Initialized = true;
    }
    bool Initialized;

    void Update()
    {
        Initialize();

        if (Time.time < 5f)// Wait a few seconds.
        {
            return;
        }
        enabled = false;

        // Create join room request.
        GenericTag joinTag = new GenericTag(new XmlQualifiedName("presence"));
        // XmlAttribute fromAttribute = joinTag.OwnerDocument.CreateAttribute("", "from", "");
        // XmlAttribute toAttribute = joinTag.OwnerDocument.CreateAttribute("", "to", "10ktactics@chat.livecoding.tv");
        // joinTag.SetAttributeNode(fromAttribute);
        // joinTag.SetAttributeNode(toAttribute);

        joinTag.SetAttribute("from", "10kbot@livecoding.tv/10kbot");
        joinTag.SetAttribute("id", "ng91xs69");
        joinTag.SetAttribute("to", "10ktactics@chat.livecoding.tv/10ktactics");

        LogQueue.Log(Time.frameCount + " Attempting to send tag " + joinTag.Value);

        // Send it.
        // LivecodingXmpp.Send(joinTag);

        /*joinTag.Attributes

        if (joinRoomXml.Attributes == null)
        {
            LogQueue.Log("Attributes is null.");
        }
        else
        {
            foreach (XmlAttribute att in joinRoomXml.Attributes)
                joinTag.Attributes.Append(att);
        }

        joinTag.ParentNode.Attributes.Append()

        LogQueue.Log("joinTag has xml " + joinTag.Value);

        // How do I get my jid and the room's jid into the joinTag?*/



        // XmlQualifiedName presense = new XmlQualifiedName("presence");

        // GenericTag joinRoomTagMaybe = TagRegistry.GetTag<GenericTag>();
        // XmlAttribute fromAttribute = joinRoomTagMaybe.Cre("from");

        // Need to add to, from attributes. Maybe ID attribute I don't know.

        // LivecodingXmpp.Send()
    }

    private void HandleNewTag(object sender, TagEventArgs e)
    {
        if (e != null)
            LogQueue.Log("Received new tag from " + sender);
        else
            LogQueue.Log("Received new tag " + e.Tag + " from " + sender);
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
