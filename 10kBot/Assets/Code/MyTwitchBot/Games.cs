/*using System;
using System.Collections.Generic;
using System.Linq;
namespace MTB
{
    class Games
    {
        public static bool Enabled = false;

        public class EightBall
        {
            public static bool Enabled = false;
            private static string[] Anwers = 
            { "It is certain", "It is decidedly so", "Without a doubt" , "Yes, definitely" , "You may rely on it", "As I see it, yes", "Most likely", "Outlook good", "Yes", "Signs point to yes",
              "Reply hazy try again", "Ask again later", "Better not tell you now", "Cannot predict now", "Concentrate and ask again",
              "Don't count on it", "My reply is no", "My sources say no", "Outlook not so good", "Very doubtful" };

            public static void Ask()
            {
                if (Enabled)
                {
                    Random Rand = new Random();
                    int Number = Rand.Next(Anwers.Length);
                    Networking.SendChatMessage(Anwers[Number]);
                }
                else
                {
                    Networking.SendChatMessage("Sorry, but 8Ball is currently not active.");
                }
            }
        }

        public class RussianRoulette
        {
            public static bool Enabled = false;

            /// <summary>
            /// Roll the wheel and shoot
            /// </summary>
            public static void Roll()
            {
                if (Enabled)
                {
                    Random Rand = new Random();
                    int Number = Rand.Next(3);
                    if (Number == 0)
                    {
                        Networking.SendChatMessage("Klick -> Bang - Now you're dead. Better Luck next time!");
                    }
                    else
                    {
                        Networking.SendChatMessage("Klick -> Klick - Lucky guy. Wanna try again?");
                    }
                }
                else
                {
                    Networking.SendChatMessage("Sorry, but RussianRoulette is currently not active.");
                }
            }
        }
    }
}
*/