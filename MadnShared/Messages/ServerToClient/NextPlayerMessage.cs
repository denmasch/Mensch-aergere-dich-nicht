using System.Text.Json.Serialization;
using MadnShared.Enums;
using MadnShared.Messages.Base;

namespace MadnShared.Messages.ServerToClient;

/// <summary>
/// The next player is up
/// </summary>
public class NextPlayerMessage : IGameMessage
{
    public string Type => MessageType.NextPlayer;
        
    public Guid GameId { get; set; }

    public Guid NextPlayerId { get; set; }
    
    [JsonConverter(typeof(JsonStringEnumConverter))]    
    public Color NextPlayerColor { get; set; }
}