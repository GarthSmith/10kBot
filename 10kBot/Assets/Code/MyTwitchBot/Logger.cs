using System;
using System.IO;
using UnityEngine;

namespace MTB
{
    public class Logger
    {
        private static string getCurrentTime()
        {
            var now = DateTime.Now;
            return now.ToString("dd.MM.yyyy H:mm");
        }

        private static string getCurrentLogTime()
        {
            var now = DateTime.Now;
            return now.ToString("dd.MM.yyyy_H.mm.ss");
        }

        /// <summary>
        /// Loggs an action into the console window
        /// </summary>
        /// <param name="type">INFO, WARNING, ERROR, TWITCH, CHAT</param>
        /// <param name="message">Message to display at the console</param>
        public static void Log(string type, string message)
        {
            if (message.ToLower().Contains("pass")) return;

            var log = getCurrentTime() + " >> [" + type.ToUpper() + "] " + message;
            
            type = type.ToLower();
            switch (type)
            {
                case "info":
                    // Debug.Log(log);
                    break;

                case "warning":
                    Debug.LogWarning(log);
                    break;

                case "error":
                    Debug.LogError(log);
                    break;

                case "twitch":
                    Debug.Log(log);
                    break;

                case "chat":
                    Debug.Log(log);
                    break;

                default:
                    Debug.LogWarning(log);
                    break;
            }
        }
    }
}
