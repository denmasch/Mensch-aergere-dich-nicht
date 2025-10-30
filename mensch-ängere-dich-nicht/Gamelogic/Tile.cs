namespace mensch_ängere_dich_nicht.Gamelogic;

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
    public Figure? OccupyingFigure { get; set; }
    
    public bool IsOccupied => OccupyingFigure != null;
    
    public TileType Type { get; private set; }

    public Color? Color { get; private set; }
}