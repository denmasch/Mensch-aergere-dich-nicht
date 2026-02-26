using System.Text.Json.Serialization;
using MadnShared.Enums;
using MadnShared.Messages.Base;

namespace MadnShared.Messages.ClientToServer;

/// <summary>
/// The player wants to roll the dice
/// </summary>
public class RollDiceMessage : IGameMessage
{
    public string Type => MessageType.RollDice;
        
    public Guid GameId { get; set; }
    
    public string PlayerId { get; set; }  = "";
}