namespace mensch_ängere_dich_nicht.Gamelogic;

public class Gameboard
{
    public Gameboard()
    {
        InitializePath();
        InitializeHomes();
        InitializeTargets();
        PlaceFigures();
    }

    public Tile[] Path { get; set; } = new Tile[40];

    public Dictionary<Color, Tile[]> Homes { get; set; } = new();

    public Dictionary<Color, Tile[]> Targets { get; set; } = new();

    private void InitializePath()
    {
        for (int i = 0; i < Path.Length; i++)
        {
            if (i % 10 == 0)
            {
                Path[i] = new Tile(TileType.Start);
            }
            else
            {
                Path[i] = new Tile(TileType.Normal);
            }
        }
    }
    
    private void InitializeHomes()
    {
        foreach (Color color in Enum.GetValues(typeof(Color)))
        {
            Tile[] home = new Tile[4];
            for (int i = 0; i < home.Length; i++)
            {
                home[i] = new Tile(TileType.Home);
            }
            Homes.Add(color, home);
        }
    }

    private void InitializeTargets()
    {
        foreach (Color color in Enum.GetValues(typeof(Color)))
        {
            Tile[] target = new Tile[4];
            for (int i = 0; i < target.Length; i++)
            {
                target[i] = new Tile(TileType.Target);
            }
            Targets.Add(color, target);
        }
    }

    /// <summary>
    /// Place figures in the home area of their respective color
    /// </summary>
    private void PlaceFigures()
    {
        foreach (Color color in Homes.Keys)
        {
            var home = Homes[color];
            foreach (Tile tile in home)
            {
                tile.OccupyingFigure = new Figure(color);
            }
        }
    }
}