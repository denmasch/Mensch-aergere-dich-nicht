using MadnShared.Messages.Base;

namespace MadnShared.Messages.ServerToClient;

/// <summary>
/// Successfully created a new Game, the Player is the Host of the Game
/// </summary>
public class GameCreatedMessage : GameMessage
{
    public string GameId { get; set; }  = "";
    
    public string PlayerId { get; set; } = "";
    
    public GameCreatedMessage()
    {
        Type = MessageType.GameCreated;
    }
}