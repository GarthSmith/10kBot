using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
// using System.Threading;
using System.Collections;

namespace MTB
{
    public class Networking
    {
        public static Twitch Twitch;
        static Networking()
        {
            // Twitch = new Twitch();
        }

        private static TcpClient ClientSocket = new TcpClient();

        private static bool ShouldStop = false;

        private static StreamReader SocketStreamReceiver;
        private static StreamWriter SocketStreamWriter;

        // private static AutoResetEvent SenderResetEvent = new AutoResetEvent(false);

        private static int SocketIndex = 0;

        private static List<string> SocketCommands = new List<string>();

        // private static WMPLib.WindowsMediaPlayer wplayer = new WMPLib.WindowsMediaPlayer();

        #region Public functions

        /// <summary>
        /// Adds a static/plain command to the messagequeue
        /// </summary>
        /// <param name="command">Command to add</param>
        public static void AddStaticCommand(string command)
        {
            SocketCommands.Add(command);
        }

        /// <summary>
        /// Connects to the Twitch IRC Server
        /// </summary>
        public static void Connect()
        {
            try
            {
                ClientSocket.Connect(Twitch.Server, Twitch.Port);
                Twitch.StartCoroutine(GetMessageFromServer());
                Twitch.StartCoroutine(SendMessageToServer());
            }
            catch (Exception ex)
            {
                Logger.Log("error", ex.StackTrace);
                Reconnect();
            }
        }

        /// <summary>
        /// Sends all queued commands to the server
        /// </summary>
        public static void Flush()
        {
           //  SenderResetEvent.Set();
        }

        /// <summary>
        /// Reconnects to the Twitch IRC Server
        /// </summary>
        public static void Reconnect()
        {
            try
            {
                if (ClientSocket.Connected)
                {
                    ClientSocket.Close();
                }
                ClientSocket = null;

                Twitch.StopCoroutine(GetMessageFromServer());
                Twitch.StopCoroutine(SendMessageToServer());

                Connect();
            }
            catch (Exception ex)
            {
                Logger.Log("error", ex.StackTrace);
            }
        }

        /// <summary>
        /// Send a text directly to the chat
        /// </summary>
        /// <param name="Message">Message to Send</param>
        public static void SendChatMessage(string Message)
        {
            SocketCommands.Add("PRIVMSG " + Twitch.TwitchChannel + " :" + Message);
            Flush();
        }

        #endregion

        #region Private functions

        private static void ParseChatLine(string ChatLine)
        {
            if (ChatLine.StartsWith(":"))
            {
                if (ChatLine.Contains("PRIVMSG"))
                {
                    if (Twitch.Message != null)
                        Twitch.Message(ChatLine);
                    int pos = ChatLine.IndexOf(Twitch.TwitchChannel) + Twitch.TwitchChannel.Length + 2;
                    string message = ChatLine.Substring(pos, ChatLine.Length - pos);
                    string user = ChatLine.Substring(1, ChatLine.IndexOf('!') - 1);
                    if (!message.ToLower().Contains("pass"))
                        Logger.Log("chat", user + ": " + message);

                    //string path = System.IO.Directory.GetCurrentDirectory() + "\\sounds\\message.mp3";
                    //wplayer.URL = path;
                    //wplayer.controls.play();

                    if (Games.Enabled)
                    {
                        if (Games.RussianRoulette.Enabled)
                        {
                            if (message.StartsWith("!roll"))
                            {
                                Games.RussianRoulette.Roll();
                            }
                        }
                        if (Games.EightBall.Enabled)
                        {
                            if (message.StartsWith("!8ball"))
                            {
                                Games.EightBall.Ask();
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Private functions Coroutines

        private static IEnumerator GetMessageFromServer()
        {
            SocketStreamReceiver = new StreamReader(ClientSocket.GetStream());

            while (!ShouldStop)
            {
                try
                {
                    string Response = SocketStreamReceiver.ReadLine();
                    if (Response != null)
                    {
                        ParseChatLine(Response);
                        Logger.Log("twitch", " <= " + Response);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log("error", ex.StackTrace);
                }
                yield return null;
            }
        }

        private static IEnumerator SendMessageToServer()
        {
            SocketStreamWriter = new StreamWriter(ClientSocket.GetStream());

            while (!ShouldStop)
            {
                try
                {
                    if (SocketIndex == SocketCommands.Count)
                    {
                       //  SenderResetEvent.WaitOne();
                    }
                    else
                    {
                        Logger.Log("twitch", " => " + SocketCommands[SocketIndex]);
                        SocketStreamWriter.WriteLine(SocketCommands[SocketIndex]);
                        SocketStreamWriter.Flush();
                        SocketIndex++;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log("error", ex.StackTrace);
                }
                yield return null;
            }
        }

        #endregion
    }
}