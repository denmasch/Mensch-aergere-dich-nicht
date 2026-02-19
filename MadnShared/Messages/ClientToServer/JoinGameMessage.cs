using MadnShared.Messages.Base;

namespace MadnShared.Messages.ClientToServer;

/// <summary>
/// Join an existing Game with the Player as Participant
/// </summary>
public class JoinGameMessage : IGameMessage
{
    public string Type => MessageType.JoinGame;
    
    public string GameId { get; set; } = "";

    public string PlayerId { get; set; } = "";
}