using UnityEngine;
// using agsXMPP;
using System.IO;
using System;
// using agsXMPP.Xml.Dom;
// using agsXMPP.protocol.client;
using UnitySocketTest;

public class Livecoding : MonoBehaviour
{
    public void Start()
    {
        Debug.Log("SocketCrashTest.Start()");
        /*ClientSocket cs = new ClientSocket();
        cs.Address = "xmpp.livecoding.tv";
        cs.Port = 5222;
        cs.ConnectTimeout = 10000;
        cs.Connect();*/

        // xm = new XmppManager();
       //  xm.Go();
    }
    // XmppManager xm;

    public void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.Space))
        {
            XmppManager.Connect();
        }
        if (xm != null && xm.ProblemSocket != null && xm.ProblemSocket.ProblemSocket != null)
        {
            if (xm.ProblemSocket.ProblemSocket.Connected)
                Debug.Log("Socket is connected: " + xm.ProblemSocket.ProblemSocket.Connected);
        }*/
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
}
