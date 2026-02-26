using MadnShared.Messages.Base;

namespace MadnShared.Messages.ServerToClient;

public class GameLeftMessage : IGameMessage
{
    public string Type => MessageType.GameLeft;
}