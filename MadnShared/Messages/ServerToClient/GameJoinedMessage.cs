using MadnShared.Messages.Base;

namespace MadnShared.Messages.ServerToClient;

/// <summary>
/// Successfully joined an existing Game, the Player is a Participant of the Game
/// </summary>
public class GameJoinedMessage : GameMessage
{
    public string GameId { get; set; }  = "";
    
    public string PlayerId { get; set; } = "";
    
    public GameJoinedMessage()
    {
        Type = MessageType.GameJoined;
    }
}