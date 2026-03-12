using MadnShared.Messages.Base;

namespace MadnShared.Messages.ServerToClient;

/// <summary>
/// Successfully created a new Game, the Player is the Host of the Game
/// </summary>
public class GameCreatedMessage : IGameMessage
{
    public string Type => MessageType.GameCreated;
        
    public Guid GameId { get; set; }
    
    public Guid PlayerId { get; set; }
}