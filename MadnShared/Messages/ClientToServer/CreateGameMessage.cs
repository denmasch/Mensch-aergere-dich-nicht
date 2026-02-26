using System.Text.Json.Serialization;
using MadnShared.Enums;
using MadnShared.GameAssets;
using MadnShared.Messages.Base;

namespace MadnShared.Messages.ClientToServer;

/// <summary>
/// Creates a new Game with the Player as Host
/// </summary>
public class CreateGameMessage : IGameMessage
{
    public string Type => MessageType.CreateGame;
    public string PlayerId { get; set; } = "";
}