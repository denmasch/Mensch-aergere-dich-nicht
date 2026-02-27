using MadnShared.Messages.Base;

namespace MadnShared.Messages.ClientToServer;

/// <summary>
/// Join an existing Game with the Player as Participant
/// </summary>
public class LeaveGameMessage : IGameMessage
{
    public string Type => MessageType.LeaveGame;
    
    public Guid GameId { get; set; }

    public Guid PlayerId { get; set; }
}