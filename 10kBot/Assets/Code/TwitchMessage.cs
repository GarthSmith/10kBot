using System;
using System.Diagnostics.CodeAnalysis;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "IdentifierTypo")]
public enum Message
{
    /// <summary>
    /// Send this message to join a channel. See https://dev.twitch.tv/docs/irc/join-chat-room
    /// </summary>
    JOIN,
    
    /// <summary>
    /// Nickname. Sends this message to specify nickname when authenticating with the Twitch IRC server.
    /// See https://dev.twitch.tv/docs/irc/authenticate-bot
    /// </summary>
    NICK,
    
    /// <summary>
    /// We receive this message from the Twitch IRC server when your bot fails to authenticate with the server. You can get NOTICE messages for other reasons if you request the commands capability.
    /// </summary>
    NOTICE,
    
    /// <summary>
    /// Bot sends this message to leave a channel.
    /// Bot receives this message from the Twitch IRC server when a channel bans it.
    /// </summary>
    PART,
    
    /// <summary>
    /// Password. Bot sends this message to specify the bot’s password when authenticating with the Twitch IRC server.
    /// See https://dev.twitch.tv/docs/irc/authenticate-bot
    /// </summary>
    PASS,
    
    /// <summary>
    /// Bot receives this message from the Twitch IRC server when the server wants to ensure that your bot is still alive and able to respond to the server’s messages.
    /// See https://dev.twitch.tv/docs/irc/#keepalive-messages
    /// </summary>
    PING,
    
    /// <summary>
    /// Bot sends this message in reply to the Twitch IRC server’s PING message.
    /// See https://dev.twitch.tv/docs/irc/#keepalive-messages
    /// </summary>
    PONG,
    
    /// <summary>
    /// Bot sends this message to post a chat message in the channel’s chat room.
    /// Bot receives this message from the Twitch IRC server when a user posts a chat message in the chat room.
    /// See https://dev.twitch.tv/docs/irc/send-receive-messages
    /// </summary>
    PRIVMSG,
    
    
    CLEARCHAT,
    CLEARMSG,
    GLOBALUSERSTATE,
    HOSTTARGET,
    RECONNECT,
    ROOMSTATE,
    USERNOTICE,
    USERSTATE,
    WHISPER
}

[Flags]
public enum MessageFlow
{
    None = 0b_0000_0000,
    Send = 0b_0000_0001,
    Receive = 0b_0000_0010,
    SendAndReceive = Send | Receive
}

public struct TwitchMessage
{
    public Message Message { get; private set; }
    public MessageFlow MessageFlow { get; private set; }

    public TwitchMessage(Message message, MessageFlow messageFlow)
    {
        Message = message;
        MessageFlow = messageFlow;
        ValidateMessageAndMessageFlowCombination(Message, MessageFlow);
    }

    private static void ValidateMessageAndMessageFlowCombination(Message message, MessageFlow messageFlow)
    {
        switch (message)
        {
            // Only Send
            case Message.JOIN:
            case Message.NICK:
            case Message.PASS:
            case Message.PONG:
                if (messageFlow != MessageFlow.Send)
                    throw new ArgumentException($"{nameof(message)} {message} must have {nameof(messageFlow)} {MessageFlow.Send} but it was {messageFlow}");
                break;
            
            // Only Receive
            case Message.NOTICE:
            case Message.PING:
            case Message.CLEARCHAT:
            case Message.CLEARMSG:
            case Message.GLOBALUSERSTATE:
            case Message.HOSTTARGET:
            case Message.RECONNECT:
            case Message.ROOMSTATE:
            case Message.USERNOTICE:
            case Message.USERSTATE:
            case Message.WHISPER:
                if (messageFlow != MessageFlow.Receive)
                    throw new ArgumentException($"{nameof(message)} {message} must have {nameof(messageFlow)} {MessageFlow.Receive} but it was {messageFlow}");
                break;
            
            // Can do either
            case Message.PART:
            case Message.PRIVMSG:
                var forSending = (messageFlow & MessageFlow.Send) != 0;
                var forReceiving = (messageFlow & MessageFlow.Receive) != 0;
                if (!forSending && !forReceiving)
                    throw new ArgumentException($"{nameof(message)} {message} must have {nameof(messageFlow)} {MessageFlow.Receive} and/or {MessageFlow.Send} but it was {messageFlow}");
                break;
            
            default:
                throw new ArgumentOutOfRangeException(nameof(message), message, null);
        }
    }
}