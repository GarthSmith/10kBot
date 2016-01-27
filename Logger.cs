using System;
using System.IO;

namespace MTB
{
    public class Logger
    {
        private static StreamWriter file;
        private static string FileName = String.Empty;

        static string getCurrentTime()
        {
            DateTime now = DateTime.Now;
            return now.ToString("dd.MM.yyyy H:mm");
        }

        static string getCurrentLogTime()
        {
            DateTime now = DateTime.Now;
            return now.ToString("dd.MM.yyyy_H.mm.ss");
        }

        /// <summary>
        /// Loggs an action into the console window
        /// </summary>
        /// <param name="type">INFO, WARNING, ERROR, TWITCH, CHAT</param>
        /// <param name="message">Message to display at the console</param>
        public static void Log(string type, string message)
        {
            string log = string.Empty;
            if (FileName == String.Empty)
            {
                bool exists = System.IO.Directory.Exists("./logs");
                if (!exists)
                    System.IO.Directory.CreateDirectory("./logs");

                FileName = "./logs/debug_" + getCurrentLogTime() + ".txt";
            }
            file = new StreamWriter(FileName, true);
            type = type.ToLower();
            switch (type)
            {
                case "info":
                    Console.ForegroundColor = ConsoleColor.White;
                    break;

                case "warning":
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;

                case "error":
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;

                case "twitch":
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    break;

                case "chat":
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    break;

                default:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
            }

            log = getCurrentTime() + " >> [" + type.ToUpper() + "] " + message;
            if (type != "twitch")
                Console.WriteLine(log);
            file.WriteLine(log);
            file.Close();
        }
    }
}
