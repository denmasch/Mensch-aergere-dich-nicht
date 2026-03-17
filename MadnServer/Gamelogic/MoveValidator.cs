namespace MadnServer.Gamelogic;
using MadnShared.Enums;

public class MoveValidator
{
    public bool ValidateMove(Gameboard gb, Figure fig, Color activePlayer, int diceRollCount )
    {
        if (fig.Color != activePlayer || diceRollCount <= 0)
        {
            return false;
        }

        //TODO: Test if Movement is allowed
        
        /* Allowed Movement:
        - If 6 is Rolled allow movement again
        - Figure moves from Tile of Path to empty Tile of
            - Path
            - own-color Targets 
                - if diceRollCount does not exceed Targets
        - Figure moves within Target Tiles to empty Tile of Target of its own color
        - Figure captures enemy Figure
        - Figures gets out of Home Tiles to Path by rolling 6
            - only if Path Tile
                - is empty
                - is enemy figures -> gets captured
        */
        
        // Check Movement out of Home 
        if (fig.IsHome)
        {
            return ValidateHomeExit(gb, activePlayer, diceRollCount);
        }
        // Check Movement within Target
        if (TryFindCurrentTargetIndex(gb, fig, activePlayer, out int currentTargetIndex))
        {
            return ValidateTargetMove(gb, activePlayer, currentTargetIndex, diceRollCount);
        }
        // Is Figure on Board? 
        if (!TryFindCurrentPathIndex(gb, fig, out int currentTile))
        {
            return false;
        }
        // Is Valid Start on Board?
        if (!TryFindPlayerStartIndex(gb, activePlayer, out int playerStartIndex))
        {
            return false;
        }
        // Check Movement for entering Target
        if (TryValidateTargetEntry(gb, activePlayer, currentTile, playerStartIndex, diceRollCount, out bool targetMoveAllowed))
        {
            return targetMoveAllowed;
        }
        // Check Movement within Path 
        return ValidatePathMove(gb, currentTile, activePlayer, diceRollCount);
    }

    private bool ValidateHomeExit(Gameboard gb, Color activePlayer, int diceRollCount)
    {
        if (diceRollCount != 6)
        {
            return false;
        }

        for (int i = 0; i < Gameboard.PathLength; i++)
        {
            if (gb.Path[i].Color == activePlayer && gb.Path[i].Type == TileType.Start)
            {
                return IsTileFree(gb.Path[i], activePlayer);
            }
        }

        return false;
    }

    private bool TryFindCurrentPathIndex(Gameboard gb, Figure fig, out int currentTile)
    {
        return TryFindFigureOnTiles(gb.Path, fig, out currentTile);
    }

    private bool TryFindCurrentTargetIndex(Gameboard gb, Figure fig, Color activePlayer, out int currentTargetIndex)
    {
        if (!TryGetPlayerTargets(gb, activePlayer, out Tile[] playerTargets))
        {
            currentTargetIndex = -1;
            return false;
        }

        return TryFindFigureOnTiles(playerTargets, fig, out currentTargetIndex);
    }

    private bool TryFindPlayerStartIndex(Gameboard gb, Color activePlayer, out int playerStartIndex)
    {
        for (int i = 0; i < gb.Path.Length; i++)
        {
            if (gb.Path[i].Type == TileType.Start && gb.Path[i].Color == activePlayer)
            {
                playerStartIndex = i;
                return true;
            }
        }

        playerStartIndex = -1;
        return false;
    }

    private bool TryValidateTargetEntry(Gameboard gb, Color activePlayer, int currentTile, int playerStartIndex, int diceRollCount, out bool isAllowed)
    {
        int stepsToStart = GetStepsToStart(currentTile, playerStartIndex, gb.Path.Length);
        if (diceRollCount <= stepsToStart)
        {
            isAllowed = false;
            return false;
        }

        if (!TryGetPlayerTargets(gb, activePlayer, out Tile[] playerTargets))
        {
            isAllowed = false;
            return true;
        }

        int targetIndex = diceRollCount - stepsToStart - 1;
        if (targetIndex < 0 || targetIndex >= playerTargets.Length)
        {
            isAllowed = false;
            return true;
        }

        isAllowed = AreTargetTilesFree(playerTargets, -1, targetIndex);
        return true;
    }

    private bool ValidateTargetMove(Gameboard gb, Color activePlayer, int currentTargetIndex, int diceRollCount)
    {
        if (!TryGetPlayerTargets(gb, activePlayer, out Tile[] playerTargets))
        {
            return false;
        }

        int newTargetIndex = currentTargetIndex + diceRollCount;
        if (newTargetIndex >= playerTargets.Length)
        {
            return false;
        }

        return AreTargetTilesFree(playerTargets, currentTargetIndex, newTargetIndex);
    }

    private int GetStepsToStart(int currentTile, int playerStartIndex, int pathLength)
    {
        int stepsToStart = (playerStartIndex - currentTile + pathLength) % pathLength;
        return stepsToStart == 0 ? pathLength : stepsToStart;
    }

    private bool TryGetPlayerTargets(Gameboard gb, Color activePlayer, out Tile[] playerTargets)
    {
        if (gb.Targets.ContainsKey(activePlayer))
        {
            playerTargets = gb.Targets[activePlayer];
            return true;
        }

        playerTargets = [];
        return false;
    }

    private bool TryFindFigureOnTiles(Tile[] tiles, Figure fig, out int tileIndex)
    {
        for (int i = 0; i < tiles.Length; i++)
        {
            if (tiles[i].OccupyingFigure == fig)
            {
                tileIndex = i;
                return true;
            }
        }

        tileIndex = -1;
        return false;
    }

    private bool AreTargetTilesFree(Tile[] playerTargets, int startExclusive, int endInclusive)
    {
        for (int i = startExclusive + 1; i <= endInclusive; i++)
        {
            if (playerTargets[i].IsOccupied)
            {
                return false;
            }
        }

        return true;
    }

    private bool ValidatePathMove(Gameboard gb, int currentTile, Color activePlayer, int diceRollCount)
    {
        int newTile = (currentTile + diceRollCount) % gb.Path.Length;
        return IsTileFree(gb.Path[newTile], activePlayer);
    }

    private bool IsTileFree(Tile tile,  Color activePlayer)
    {
        return !tile.IsOccupied || tile.OccupyingFigure?.Color != activePlayer;
    }
}