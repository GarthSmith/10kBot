

using System;
namespace MTB
{
    class Programm
    {
        static void Main(string[] args)
        {
            Console.Title = "My Twitch Bot";
            Networking.Connect();

            Twitch.Login();

            string cmd = String.Empty;

            Games.Enabled = true;
            Games.RussianRoulette.Enabled = true;
            Games.EightBall.Enabled = true;

            while(cmd != ".exit")
            {
                cmd = Console.ReadLine();
                if(cmd.Contains(".say"))
                {
                    Networking.SendChatMessage(cmd.Substring(5, cmd.Length - 5));
                }
            }
            
        }
    }
}
