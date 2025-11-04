using MadnShared.Messages.Base;

namespace MadnShared.Messages.ClientToServer;

public class JoinGameMessage : GameMessage
{
    public string GameId { get; set; } = "";
    
    public string PlayerId { get; set; } = "";
    
    public JoinGameMessage()
    {
        Type = MessageType.JoinGame;
    }
}