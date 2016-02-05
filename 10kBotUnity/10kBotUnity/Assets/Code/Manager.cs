using UnityEngine;
using UnityEngine.UI;

public class Manager : MonoBehaviour
{
    public Text ChannelText;
    TwitchIrc Twitch;
    Livecoding Livecoding;

    void Awake()
    {
        Livecoding = gameObject.AddComponent<Livecoding>();
        Twitch = gameObject.AddComponent<TwitchIrc>();
    }

    protected void OnEnable()
    {
        Livecoding.MessageReceived += AppendMessage;
        Twitch.MessageReceived += AppendMessage;
    }

    protected void OnDisable()
    {
        Livecoding.MessageReceived -= AppendMessage;
        Twitch.MessageReceived -= AppendMessage;
    }

    private void AppendMessage(string name, string message)
    {
        ChannelText.text += "\n" + name + ": " + message;
    }
}