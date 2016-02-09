using UnityEngine;
using UnityEngine.UI;

public class Manager : MonoBehaviour
{
    public Text ChannelText;

    public bool TwitchOnStart;
    public bool LivecodingOnStart;

    Twitch Twitch;
    Livecoding Livecoding;

    void Awake()
    {
        if (LivecodingOnStart)
        {
            Livecoding = gameObject.AddComponent<Livecoding>();
            Livecoding.enabled = LivecodingOnStart;
        }
        if (TwitchOnStart)
        {
            Twitch = gameObject.AddComponent<Twitch>();
            Twitch.enabled = TwitchOnStart;
        }
    }

    protected void OnEnable()
    {
        if (Livecoding != null)
            Livecoding.MessageReceived += AppendMessage;
        if (Twitch != null)
        {
            Debug.Log("Manager is listening to Twitch.");
            Twitch.MessageReceived += AppendMessage;
        }
    }

    protected void OnDisable()
    {
        if (Livecoding != null)
            Livecoding.MessageReceived -= AppendMessage;
        if (Twitch != null)
            Twitch.MessageReceived -= AppendMessage;
    }

    private void AppendMessage(string name, string message)
    {
        Debug.Log("Manager is appending '" + name + ": " + message + "'");
        // Trim to make sure UI Text doesn't get too large.
        string chatText = ChannelText.text;
        chatText += name + ": " + message + "\n";
        if (chatText.Length > 64000)
            chatText = chatText.Substring(chatText.Length - 64000);
        ChannelText.text = chatText;
    }
}