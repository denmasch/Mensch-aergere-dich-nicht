using System.Text.Json.Serialization;
using MadnShared.Enums;

namespace MadnShared.GameAssets;

public class FigureDTO
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Color Color { get; private set; }
    
    public int Id { get; private set; }
}