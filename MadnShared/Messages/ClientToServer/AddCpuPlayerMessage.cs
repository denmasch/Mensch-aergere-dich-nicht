using System.Text.Json.Serialization;
using MadnShared.Enums;
using MadnShared.Messages.Base;

namespace MadnShared.Messages.ClientToServer;

public class AddCpuPlayerMessage : IGameMessage
{
    public string Type => MessageType.AddCpuPlayer;
    
    public Guid GameId { get; set; }
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Difficulty Difficulty { get; set; }
}