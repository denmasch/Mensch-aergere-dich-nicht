using System.Text.Json.Serialization;
using MadnShared.Enums;
using MadnShared.GameAssets;
using MadnShared.Messages.Base;

namespace MadnShared.Messages.ClientToServer;

/// <summary>
/// Player moves Figure by x Tiles
/// </summary>
public class MoveFigureMessage : GameMessage
{
    public string PlayerId { get; set; } 
    
    public Figure Figure { get; set; }
    
    public int Tiles { get; set; } 

    public MoveFigureMessage()
    {
        Type = MessageType.MoveFigure;
    }
}