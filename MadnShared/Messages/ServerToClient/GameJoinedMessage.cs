using MadnShared.Messages.Base;

namespace MadnShared.Messages.ServerToClient;

/// <summary>
/// Successfully joined an existing Game, the Player is a Participant of the Game
/// </summary>
public class GameJoinedMessage : IGameMessage
{
    public string Type => MessageType.GameJoined;
    
    public string GameId { get; set; }  = "";
    
    public string PlayerId { get; set; } = "";
}