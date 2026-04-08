using MadnShared.Enums;

namespace MadnShared.GameAssets;

public class GameboardDTO
{
    public TileDTO[] Path { get; set; }
    public Dictionary<Color, TileDTO[]> Homes { get; set; }
    public Dictionary<Color, TileDTO[]> Targets { get; set; }
}