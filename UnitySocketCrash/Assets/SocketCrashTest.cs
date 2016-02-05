using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;

public class SocketCrashTest : MonoBehaviour
{
    void Start()
    {
        Debug.Log("SocketCrashTest.Start()");
        ClientSocket cs = new ClientSocket();
        cs.Address = "xmpp.livecoding.tv";
        cs.Port = 5222;
        cs.ConnectTimeout = 10000;
        cs.Connect();
    }


    /*
    void Awake()
    {
        IPHostEntry ipHostInfo = Dns.GetHostEntry("livecoding.tv");
        IPAddress ipAddress = ipHostInfo.AddressList[0];// IPAddress.Parse(address);
        IPEndPoint endPoint = new IPEndPoint(ipAddress, 5222);

        // LogQueue.Log("Attempting to connect socket.");

        var _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        try
        {
            Debug.Log("Going to connect to " + endPoint);
            _socket.Connect(endPoint);
            Debug.Log("Connected");
        }
        catch (Exception e)
        {
            Debug.LogError("Unable to connect got " + e + ": " + e.Message);
        }

       //  LivecodingXmpp = new Xmpp();
    }
    */
}
