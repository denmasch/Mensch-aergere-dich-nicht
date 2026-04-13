using System.Text.Json.Serialization;
using MadnShared.Enums;
using MadnShared.Messages.Base;

namespace MadnShared.Messages.ServerToClient;

public class GameOverMessage :  IGameMessage
{
    public string Type => MessageType.GameOver;
    
    public Guid GameId { get; set; }
    
    public Guid WinnerPlayerId { get; set; }
        
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Color WinnerColor { get; set; }
}