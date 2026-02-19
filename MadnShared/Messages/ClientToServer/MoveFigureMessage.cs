using System.Text.Json.Serialization;
using MadnShared.Enums;
using MadnShared.GameAssets;
using MadnShared.Messages.Base;

namespace MadnShared.Messages.ClientToServer;

/// <summary>
/// Player moves Figure by x Tiles
/// </summary>
public class MoveFigureMessage : IGameMessage
{
    public string Type => MessageType.MoveFigure;
    
    public string PlayerId { get; set; }  = "";
    
    public int FigureId { get; set; }
    
    public int Tiles { get; set; } 
}