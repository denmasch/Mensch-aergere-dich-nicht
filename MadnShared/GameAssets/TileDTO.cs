using System.Text.Json.Serialization;
using MadnShared.Enums;

namespace MadnShared.GameAssets;

public class TileDTO
{
    public FigureDTO? OccupyingFigure { get; set; }
    
    public bool IsOccupied => OccupyingFigure != null;
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TileType Type { get; private set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Color Color { get; private set; }
}