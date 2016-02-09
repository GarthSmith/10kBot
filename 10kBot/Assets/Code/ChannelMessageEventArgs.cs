using System;

public class ChannelMessageEventArgs : EventArgs
{
    public string From { get; internal set; }
    public string Message { get; internal set; }

    public ChannelMessageEventArgs(string Channel, string From, string Message)
    {
        this.From = From;
        this.Message = Message;
    }
}