using System;
using System.Collections.Generic;
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
}