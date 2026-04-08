using System.Text.Json.Serialization;
using MadnShared.Enums;

namespace MadnShared.GameAssets;

public class FigureDTO
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Color Color { get; set; }
    
    public int Id { get; set; }
}