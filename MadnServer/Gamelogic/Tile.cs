using MadnShared.Enums;
using MadnShared.GameAssets;

namespace MadnServer.Gamelogic;

public class Tile
{
    public Tile(TileType tileType)
    {
        Type = tileType;
    }
    
    public Tile(TileType tileType, Color color)
    {
        Type = tileType;
        Color = color;
    }
    
    public TileDTO toDto()
    {
        return new TileDTO
        {
            Type = this.Type,
            Color = this.Color,
            OccupyingFigure = this.OccupyingFigure?.toDto()
        };
    }
    
    public Figure? OccupyingFigure { get; set; }
    
    public bool IsOccupied => OccupyingFigure != null;
    
    public TileType Type { get; private set; }

    public Color Color { get; private set; }
}