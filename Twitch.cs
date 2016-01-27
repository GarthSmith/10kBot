using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTB
{
    public class Twitch
    {
        public static string Server = "irc.twitch.tv";
        public static int Port = 6667;

        public static string TwitchUser = "";
        private static string TwitchPassword = "";
        public static string TwitchChannel = "";

        public static void Login()
        {
            Networking.AddStaticCommand("PASS " + TwitchPassword);
            Networking.AddStaticCommand("NICK " + TwitchUser);
            Networking.AddStaticCommand("CAP REQ :twitch.tv/membership");
            Networking.AddStaticCommand("JOIN " + TwitchChannel);
            Networking.Flush();
        }
    }
}
