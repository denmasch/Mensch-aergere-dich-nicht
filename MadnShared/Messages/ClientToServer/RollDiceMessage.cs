using System.Text.Json.Serialization;
using MadnShared.Enums;
using MadnShared.Messages.Base;

namespace MadnShared.Messages.ClientToServer;

public class RollDiceMessage : GameMessage
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Color Color { get; set; } 

    public RollDiceMessage()
    {
        Type = MessageType.RollDice;
    }
}