using UnityEngine;
using UnityEngine.UI;
using MTB;
using System.Linq;

public class Manager : MonoBehaviour
{
    public Text ChannelText;
    //public Twitch Twitch;

    public void OnEnable()
    {
        //Twitch.Message += AppendMessage;
        // Twitch.Login();
    }

    private void AppendMessage(string obj)
    {
        ChannelText.text += "\n" + obj;
    }

    public void Disconnect()
    {
        //Twitch.LogOut();
        //Twitch.Message -= AppendMessage;
    }
}