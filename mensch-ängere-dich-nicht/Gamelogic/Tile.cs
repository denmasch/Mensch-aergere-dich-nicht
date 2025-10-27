namespace mensch_ängere_dich_nicht.Gamelogic;

public class Tile
{
    public Tile(TileType tileType)
    {
        Type = tileType;
    }
    public Figure? OccupyingFigure { get; set; }
    
    public bool IsOccupied => OccupyingFigure != null;
    
    public TileType Type { get; private set; }
}