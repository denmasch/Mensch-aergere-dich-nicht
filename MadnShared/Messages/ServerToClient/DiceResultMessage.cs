using System.Text.Json.Serialization;
using MadnShared.Enums;
using MadnShared.Messages.Base;

namespace MadnShared.Messages.ServerToClient;

/// <summary>
/// The result of the dice roll
/// </summary>
public class DiceResultMessage : GameMessage
{

    public string PlayerId { get; set; } 
    
    public int Value { get; set; }
    
    public DiceResultMessage()
    {
        Type = MessageType.DiceResult;
    }
}