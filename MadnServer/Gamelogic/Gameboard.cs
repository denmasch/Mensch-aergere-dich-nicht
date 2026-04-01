using System;
using System.Collections.Generic;
using System.Linq;
using MadnShared.Enums;
using MadnShared.GameAssets;
using System.Text;

namespace MadnServer.Gamelogic;

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


    public static int PathLength { get; set; } = 40;
    private int HomeLength { get; set; } = 4;
    private int TargetLength { get; set; } = 4;

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
    
    public GameboardDTO ToDto()
    {
        //TODO: Implement toDto conversion
        return null;
    }

    public Figure GetFigure(Color color, int id)
    {
        var figures = GetAllFigures(color);
        return figures.First(f => f.Id == id);
    }
    
    public bool MoveFigure(Figure fig, Color activePlayer, int diceRollCount )
    {
        bool movementAllowed = MoveValidator.ValidateMove(this, fig, activePlayer, diceRollCount);
        if (!movementAllowed)
        {
            return false;
        }

        // Case 1: Figure is in Home (dice roll is 6)
        var currentHomeTile = FindHomeTile(fig);
        if (currentHomeTile != null && diceRollCount == 6)
        {
            // Move from Home to Start
            currentHomeTile.OccupyingFigure = null;
            var startIndex = GetStartIndexForColor(fig.Color);
            var destTile = Path[startIndex];

            if (destTile.IsOccupied)
            {
                KickFigure(destTile.OccupyingFigure);
            }

            destTile.OccupyingFigure = fig;
            fig.IsHome = false;
            return true;
        }

        // Case 2: Figure is on the Path or in the Target
        var currentPathIndex = FindPathIndex(fig);
        var currentTargetIndex = FindTargetIndex(fig);

        if (currentPathIndex >= 0)
        {
            // Figure is on the path
            var startIndex = GetStartIndexForColor(fig.Color);

            // Distanz von Start entlang des Rings zur aktuellen Position
            int distanceFromStart = (currentPathIndex - startIndex + PathLength) % PathLength;
            int totalDistanceAfterMove = distanceFromStart + diceRollCount;

            // check if the figure has moved a full lap
            if (totalDistanceAfterMove <= PathLength - 1)
            {
                // move within path
                int newIndex = (currentPathIndex + diceRollCount) % PathLength;

                var destTile = Path[newIndex];
                if (destTile.IsOccupied)
                {
                    KickFigure(destTile.OccupyingFigure);
                }

                Path[currentPathIndex].OccupyingFigure = null;
                destTile.OccupyingFigure = fig;
                return true;
            }
            else
            {
                // move into target
                int stepsIntoTarget = totalDistanceAfterMove - (PathLength - 1);
                int targetIndex = stepsIntoTarget - 1; // 0-based
                
                var targetTiles = Targets[fig.Color];
                var destTargetTile = targetTiles[targetIndex];

                Path[currentPathIndex].OccupyingFigure = null;
                destTargetTile.OccupyingFigure = fig;
                return true;
            }
        }
        else if (currentTargetIndex >= 0)
        {
            // move within target
            int newTargetIndex = currentTargetIndex + diceRollCount;
            var targetTiles = Targets[fig.Color];
            var destTargetTile = targetTiles[newTargetIndex];
            
            targetTiles[currentTargetIndex].OccupyingFigure = null;
            destTargetTile.OccupyingFigure = fig;
            
            return true;
        }

        // could not find figure
        return false;
    }


    public int GetStartIndexForColor(Color color)
    {
        return ((int)color * _armLength) % PathLength;
    }

    private Tile? FindHomeTile(Figure fig)
    {
        if (!Homes.TryGetValue(fig.Color, out var homeArr))
            return null;

        return homeArr.FirstOrDefault(t => t.OccupyingFigure == fig);
    }

    private int FindPathIndex(Figure fig)
    {
        return Array.FindIndex(Path, t => t.OccupyingFigure == fig);
    }

    private int FindTargetIndex(Figure fig)
    {
        if (!Targets.TryGetValue(fig.Color, out var targetArr))
            return -1;

        return Array.FindIndex(targetArr, t => t.OccupyingFigure == fig);
    }
    
    /// <summary>
    /// Gets all valid moves for a player based on the current gameboard state and a dice roll.
    /// </summary>
    /// <param name="color">The players color</param>
    /// <param name="diceRollCount">Dice roll</param>
    /// <returns>A list of all valid Moves</returns>
    public List<Move> GetValidMoves(Color color, int diceRollCount)
    {
        var result = new List<Move>();
        var figures = GetAllFigures(color);
        
        foreach (var f in figures)
        {
            if (MoveValidator.ValidateMove(this, f, color, diceRollCount))
            {
                result.Add(new Move
                {
                    FigureIndex = f.Id,
                    Steps = diceRollCount
                });
            }
        }

        return result;
    }

    /// <summary>
    /// Gets all figures of a player across Homes, Targets and Path.
    /// </summary>
    /// <param name="color"></param>
    /// <returns></returns>
    public List<Figure> GetAllFigures(Color color)
    {
        var figures = new List<Figure>();

        if (Homes.TryGetValue(color, out var homeArr))
            figures.AddRange(homeArr
                .Where(t => t.OccupyingFigure != null)
                .Select(t => t.OccupyingFigure!));

        if (Targets.TryGetValue(color, out var targetArr))
            figures.AddRange(targetArr
                .Where(t => t.OccupyingFigure != null)
                .Select(t => t.OccupyingFigure!));

        figures.AddRange(Path
            .Where(t => t.OccupyingFigure != null && t.OccupyingFigure.Color == color)
            .Select(t => t.OccupyingFigure!));
        
        return figures;
    }
    
    private void KickFigure(Figure fig)
    {
        // Remove figure from current tile
        var tile = GetPathTileForFigure(fig);
        if (tile != null)
        {
            tile.OccupyingFigure = null;
        }
        
        // Move figure back to home
        var home = Homes[fig.Color];
        foreach (var t in home)
        {
            if (t.OccupyingFigure == null)
            {
                t.OccupyingFigure = fig;
                fig.IsHome = true;
                break;
            }
        }
    }

    private Tile GetPathTileForFigure(Figure fig)
    {
        return Path.First(t => t.OccupyingFigure == fig);
    }
}