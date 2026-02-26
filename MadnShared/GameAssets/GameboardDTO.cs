using MadnShared.Enums;

namespace MadnShared.GameAssets;

public class GameboardDTO
{
    public TileDTO[] Path { get; set; }
    public TileDTO[] Homes { get; set; }
    public TileDTO[] Targets { get; set; }
}