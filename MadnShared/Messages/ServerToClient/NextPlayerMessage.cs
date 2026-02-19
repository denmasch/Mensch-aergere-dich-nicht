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

    public string PlayerId { get; set; }  = "";
}