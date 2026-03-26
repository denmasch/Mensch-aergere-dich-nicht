using System.Text.Json.Serialization;
using MadnShared.Enums;
using MadnShared.GameAssets;
using MadnShared.Messages.Base;

namespace MadnShared.Messages.ServerToClient;

/// <summary>
/// The result of the dice roll
/// </summary>
public class DiceResultMessage : IGameMessage
{
    public string Type => MessageType.DiceResult;
    
    public Guid PlayerId { get; set; }
        
    public Guid GameId { get; set; }
    
    public int Value { get; set; }
    
    public List<Move> ValidMoves { get; set; } = new List<Move>();
}