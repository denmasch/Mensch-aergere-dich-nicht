using MadnShared.Enums;
using System.Text;

namespace MadnShared.GameAssets;

public class Gameboard
{
    private readonly int _armLength;
    private readonly int _boardSize;
    private readonly int _halfArm;
    private readonly int _mainLine;
    private readonly int _midGap;
    private readonly int _offsetLine;
    private readonly int _endIndex;
    private readonly int _homeSideLength;


    
    private readonly Dictionary<(int, int), int> _pathCoords = new();


    private static int PathLength { get; set; } = 40;
    private static int HomeLength { get; set; } = 4;
    private static int TargetLength { get; set; } = 4;

    public Tile[] Path { get; set; } = new Tile[PathLength];
    public Dictionary<Color, Tile[]> Homes { get; set; } = new();
    public Dictionary<Color, Tile[]> Targets { get; set; } = new();
    
    
    public Gameboard()
    {
        // Validate and set dynamic layout properties
        if (PathLength % 4 != 0)
            throw new InvalidOperationException("PathLength must be divisible by 4.");
        
        _armLength = PathLength / 4;
        if (_armLength % 2 != 0)
            throw new InvalidOperationException($"PathLength / 4 (arm length) must be an even number (e.g., 8, 10, 12). Found: {_armLength}");

        _boardSize = _armLength + 1;
        _halfArm = _armLength / 2;
        _mainLine = _halfArm - 1;
        _midGap = _halfArm;
        _offsetLine = _halfArm + 1;
        _endIndex = _armLength;


        _homeSideLength = ValidateHomeLength();
        ValidateTargetLength();
        
        // Initialize Board
        InitializePathCoords();
        InitializePath();       
        InitializeHomes();      
        InitializeTargets();    
        PlaceFigures();
    }

    private int ValidateHomeLength()
    {
        double homeSqrt = Math.Sqrt(HomeLength);
        if (homeSqrt != Math.Floor(homeSqrt))
            throw new InvalidOperationException($"HomeLength must be a perfect square. Found: {HomeLength}");
        return (int)homeSqrt;
        
    }

    private void ValidateTargetLength()
    {
        int expectedTargetLength = _halfArm - 1;
        if (TargetLength != expectedTargetLength)
            throw new InvalidOperationException($"TargetLength is inconsistent. For PathLength {PathLength}, TargetLength must be (PathLength / 8) - 1 = {expectedTargetLength}. Found: {TargetLength}");
    }

    /// <summary>
    /// Builds the path coordinate map dynamically based on PathLength
    /// </summary>
    private void InitializePathCoords()
    {
        _pathCoords.Clear();
        int pathIndex = 0;

        // Arm 0
        for (int x = 0; x < _halfArm; x++)
            _pathCoords.Add((x, _mainLine), pathIndex++);
        for (int y = _mainLine - 1; y >= 0; y--)
            _pathCoords.Add((_mainLine, y), pathIndex++);
        _pathCoords.Add((_mainLine + 1, 0), pathIndex++);

        // Arm 1
        _pathCoords.Add((_offsetLine, 0), pathIndex++);
        for (int y = 1; y < _halfArm; y++)
            _pathCoords.Add((_offsetLine, y), pathIndex++);
        for (int x = _offsetLine + 1; x <= _endIndex; x++)
            _pathCoords.Add((x, _mainLine), pathIndex++);
        _pathCoords.Add((_endIndex, _mainLine + 1), pathIndex++);

        // Arm 2
        _pathCoords.Add((_endIndex, _offsetLine), pathIndex++);
        for (int x = _endIndex - 1; x >= _offsetLine; x--)
            _pathCoords.Add((x, _offsetLine), pathIndex++);
        for (int y = _offsetLine + 1; y <= _endIndex; y++)
            _pathCoords.Add((_offsetLine, y), pathIndex++);
        _pathCoords.Add((_offsetLine - 1, _endIndex), pathIndex++);
        
        // Arm 3
        _pathCoords.Add((_mainLine, _endIndex), pathIndex++); 
        for (int y = _endIndex - 1; y >= _offsetLine; y--)
            _pathCoords.Add((_mainLine, y), pathIndex++);
        for (int x = _mainLine - 1; x >= 0; x--)
            _pathCoords.Add((x, _offsetLine), pathIndex++);
        _pathCoords.Add((0, _offsetLine - 1), pathIndex++);
    }

    
    private void InitializePath()
    {
        Color color = 0;
        for (int i = 0; i < Path.Length; i++)
        {
            if (i % _armLength == 0)
            {
                Path[i] = new Tile(TileType.Start, color);
                color++;
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
            Tile[] home = new Tile[HomeLength];
            for (int i = 0; i < home.Length; i++)
            {
                home[i] = new Tile(TileType.Home, color);
            }
            Homes.Add(color, home);
        }
    }

    private void InitializeTargets()
    {
        foreach (Color color in Enum.GetValues(typeof(Color)))
        {
            Tile[] target = new Tile[TargetLength];
            for (int i = 0; i < target.Length; i++)
            {
                target[i] = new Tile(TileType.Target, color);
            }
            Targets.Add(color, target);
        }
    }

    private void PlaceFigures()
    {
        int id = 0;
        foreach (Color color in Homes.Keys)
        {
            var home = Homes[color];
            foreach (Tile tile in home)
            {
                tile.OccupyingFigure = new Figure(color, id);
                id++;
            }
        }
    }

    // --- DRAWING LOGIC ---

    /// <summary>
    /// Maps the game's Color enum to the System.ConsoleColor enum.
    /// </summary>
    /// <param name="madnColor">The game's Color enum (Red, Green, etc.)</param>
    /// <param name="dark">If true, returns the "Dark" version of the color.</param>
    /// <returns>A System.ConsoleColor</returns>
    private ConsoleColor MapColor(Color madnColor, bool dark = false)
    {
        switch (madnColor)
        {
            case Color.Red:
                return dark ? ConsoleColor.DarkRed : ConsoleColor.Red;
            case Color.Green:
                return dark ? ConsoleColor.DarkGreen : ConsoleColor.Green;
            case Color.Yellow:
                return dark ? ConsoleColor.DarkYellow : ConsoleColor.Yellow;
            case Color.Blue:
                return dark ? ConsoleColor.DarkBlue : ConsoleColor.Blue;
            default:
                return ConsoleColor.Gray; // Should not happen
        }
    }

    /// <summary>
    /// This function draws the whole board of the game 
    /// </summary>
    public void DrawBoard()
    {
        // Clear console, set encoding
        Console.Clear();
        Console.OutputEncoding = Encoding.UTF8;
        
        int boardSize = _boardSize;
        
        for (int y = 0; y < boardSize; y++)
        {
            for (int x = 0; x < boardSize; x++)
            {
                Tile tile = GetTileAt(x, y);
                string display = GetTileDisplay(tile); // " ", "1", "H", "T", "S", "."

                // Reset color for this tile
                Console.ResetColor(); 

                if (tile == null)
                {
                    Console.Write("   "); // Pad empty space for alignment
                    continue;
                }
                
                // Check if it's a "filled" tile vs an empty path tile
                bool isNotEmpty = (display != ".");

                // --- Apply Colors ---

                // 1. Figure is present
                if (tile.OccupyingFigure != null)
                {
                    // Check if tile has a color and if it matches the figure's
                    bool isDark = (tile.Type != TileType.Normal) && 
                                  tile.Color == tile.OccupyingFigure.Color;
                                  
                    Console.ForegroundColor = MapColor(tile.OccupyingFigure.Color, isDark);
                }
                // 2. No figure, Home or Target tile
                else if (tile.Type == TileType.Home || tile.Type == TileType.Target)
                {
                    Console.BackgroundColor = MapColor(tile.Color);
                }
                // 3. No figure, Start tile
                else if (tile.Type == TileType.Start)
                {
                     Console.ForegroundColor = MapColor(tile.Color);
                }
                // 4. Normal tile (".")
                
                
                if (isNotEmpty)
                {
                    // Print (X) - e.g. (1), (H), (T), (S)
                    // The color set above will apply to all 3 characters
                    Console.Write($"({display})");
                }
                else // normal tile ('.')
                {
                    Console.Write(" . "); // Pad it for alignment
                }
            }
            Console.WriteLine(); // New line after each row
        }
        // Reset color one last time at the end
        Console.ResetColor();
    }

    /// <summary>
    /// Gets the Tile object at a specific (x,y) grid coordinate.
    /// This is the "layout manager" for the board.
    /// </summary>
    /// <returns>The Tile at that location, or null if it's empty space.</returns>
    private Tile GetTileAt(int x, int y)
    {
        // --- CHECK HOMES (NxN corners, e.g., 2x2) ---
        int homeAreaEnd = _homeSideLength - 1;
        int boardAreaStart = _endIndex - homeAreaEnd;

        // Top-Left Home
        if (x <= homeAreaEnd && y <= homeAreaEnd) 
            return Homes[(Color)0][y * _homeSideLength + x];

        // Top-Right Home
        if (x >= boardAreaStart && y <= homeAreaEnd) 
            return Homes[(Color)1][y * _homeSideLength + (x - boardAreaStart)];

        // Bottom-Right Home
        if (x >= boardAreaStart && y >= boardAreaStart) 
            return Homes[(Color)2][(y - boardAreaStart) * _homeSideLength + (x - boardAreaStart)];

        // Bottom-Left Home
        if (x <= homeAreaEnd && y >= boardAreaStart) 
            return Homes[(Color)3][(y - boardAreaStart) * _homeSideLength + x];

        // --- CHECK TARGETS (1xN rows, e.g., 1x4) ---
        int targetEnd = TargetLength;
        
        // left side Targets
        if (y == _midGap && x >= 1 && x <= targetEnd) 
            return Targets[(Color)0][x - 1];

        // top side Targets
        if (x == _midGap && y >= 1 && y <= targetEnd) 
            return Targets[(Color)1][y - 1];

        // right side Targets
        int targetStart = _offsetLine; // e.g., 6
        int targetEndYellow = _endIndex - homeAreaEnd; // e.g., 10 - 1 = 9
        if (y == _midGap && x >= targetStart && x <= targetEndYellow) 
            return Targets[(Color)2][targetEndYellow - x];
        
        // bottom side Targets
        if (x == _midGap && y >= targetStart && y <= targetEndYellow) 
            return Targets[(Color)3][targetEndYellow - y];

        // check path
        if (_pathCoords.TryGetValue((x, y), out int pathIndex))
        {
            return Path[pathIndex];
        }

        // empty
        return null;
    }

    /// <summary>
    /// Gets the single-character string representation for a given tile.
    /// </summary>
    private string GetTileDisplay(Tile tile)
    {
        // Empty Space
        if (tile == null)
        {
            return " "; // 1 space
        }

        // Figure on Tile
        if (tile.OccupyingFigure != null)
        {
            int figId = tile.OccupyingFigure.Id % 4; 
            return figId.ToString(); 
        }
        
        // Empty Tile
        switch (tile.Type)
        {
            case TileType.Home:
                return "H";
            case TileType.Target:
                return "T";
            case TileType.Start:
                return "S";
            case TileType.Normal:
                return ".";
            default:
                return "?";
        }
    }
}