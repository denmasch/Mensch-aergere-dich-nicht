using System.Text.Json.Serialization;
using MadnShared.Enums;
using MadnShared.GameAssets;
using MadnShared.Messages.Base;

namespace MadnShared.Messages.ClientToServer;

/// <summary>
/// Player moves Figure by x Tiles
/// </summary>
public class CreateGameMessage : GameMessage
{
    public string PlayerId { get; set; } 

    public CreateGameMessage()
    {
        Type = MessageType.CreateGame;
    }
}