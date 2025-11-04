using MadnShared.Messages.Base;

namespace MadnShared.Messages.Errors;

public class UnknownMessageTypeMessage : GameMessage
{
    public UnknownMessageTypeMessage()
    {
        Type = MessageType.UnknownMessageType;   
    }
}