using MadnShared.Messages.Base;

namespace MadnShared.Messages.ServerToClient;

public class GameCreatedMessage : GameMessage
{
    public string GameId { get; set; } 
    
    public string PlayerId { get; set; }
    
    public GameCreatedMessage()
    {
        Type = MessageType.GameCreated;
    }
}