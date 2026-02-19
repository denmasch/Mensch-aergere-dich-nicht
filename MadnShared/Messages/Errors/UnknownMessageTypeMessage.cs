using MadnShared.Messages.Base;

namespace MadnShared.Messages.Errors;

public class UnknownMessageTypeMessage : IGameMessage
{
    public string Type => MessageType.UnknownMessageType;
}