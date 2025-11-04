using MadnShared.Messages.Base;

namespace MadnShared.Messages.ServerToClient;

public class GameJoinedMessage : GameMessage
{
    public string GameId { get; set; } 
    
    public string PlayerId { get; set; }
    
    public GameJoinedMessage()
    {
        Type = MessageType.GameJoined;
    }
}