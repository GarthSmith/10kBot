public interface IChat
{
    event System.Action<string, string> MessageReceived;
}
