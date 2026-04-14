using MadnShared.Enums;
using MadnShared.Messages.Base;

namespace MadnShared.Messages.ServerToClient;

public class GameInfoMessage : IGameMessage
{
    public string Type => MessageType.GameInfo;
    public Guid GameId { get; set; }
    
    public int PlayerCount { get; set; }
    
    public Color AdminColor { get; set; }
}