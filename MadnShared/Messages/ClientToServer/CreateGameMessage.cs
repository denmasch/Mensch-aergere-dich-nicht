using System.Text.Json.Serialization;
using MadnShared.Enums;
using MadnShared.GameAssets;
using MadnShared.Messages.Base;

namespace MadnShared.Messages.ClientToServer;

/// <summary>
/// Creates a new Game with the Player as Host
/// </summary>
public class CreateGameMessage : GameMessage
{
    public string PlayerId { get; set; } = "";

    public CreateGameMessage()
    {
        Type = MessageType.CreateGame;
    }
}