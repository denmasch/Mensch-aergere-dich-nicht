using System.Text.Json.Serialization;
using MadnShared.Enums;
using MadnShared.Messages.Base;

namespace MadnShared.Messages.ServerToClient;

public class DiceResultMessage : GameMessage
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Color Color { get; set; } 
    
    public int Value { get; set; }
    
    public DiceResultMessage()
    {
        Type = MessageType.DiceResult;
    }
}