using System.IO;

namespace MadnServer.Gamelogic;
using MadnShared.Enums;
public class MoveValidator
{
    public bool ValidateMove(Gameboard gb, Figure fig, Color activePlayer, int diceRollCount )
    {
        bool isAllowed = false;
        
        //TODO: Test if Movement is allowed
        
        /* Allowed Movement:
        - If 6 is Rolled allow movement again
        - Figure moves from Tile of Path to empty Tile of
            - Path
            - own-color Targets 
                - if diceRollCount does not exceed Targets
        - Figure captures enemy Figure
        - Figures gets out of Home Tiles to Path by rolling 6
            - only if Path Tile
                - is empty
                - is enemy figures -> gets captured
        */
        int pathLength = Gameboard.PathLength;
        // Get to Figure out of Home to Start by rolling 6 
        if (fig.IsHome == true)
        {
            if (diceRollCount == 6)
            {
                for (int i = 0; i > pathLength; i++)
                {
                    if (gb.Path[i].Color == activePlayer && gb.Path[i].Type == TileType.Start)
                    {
                        return IsTileFree(gb.Path[i], activePlayer);
                    }
                }
            }
        }
        else
        {
            //Check on which Tile is currently occupied by fig
            int i = 0;
            while (gb.Path[i].OccupyingFigure != fig)
            {
                i++;
            }

            
            int currentTile = i;
            int newTile = 0;

            
            // Check for end of array Path
            if (currentTile + diceRollCount >= gb.Path.Length - 1) // Hope this -1 correct?
            {
                newTile = (currentTile + diceRollCount) - gb.Path.Length - 1;
                bool endOfArray = true;
            }
            else
            {
                newTile = (currentTile + diceRollCount);
                bool endOfArray = false;
            }
            
            // TODO: check if Fig can go to Target or not
            //Check if Target would be skipped
            for (i = currentTile; i < newTile; i++)
            {
                // just check if fig would land on or skip Start Type Tile of the same color
                if (gb.Path[i].Type == TileType.Start && gb.Path[i].Color == activePlayer)
                {
                    //TODO: Check if Target is Occupied
                    
                }
            }

            isAllowed = IsTileFree(gb.Path[newTile], activePlayer);

        }

        return isAllowed;
    }
    

    private bool IsTileFree(Tile tile,  Color activePlayer)
    {
        if (tile.IsOccupied == true)
        {
            // Is Figure of Same color on Tile?
            if (tile.OccupyingFigure.Color == activePlayer)
            {
                return false;
            }
            else
            {
                // Figure can capture
                return true;
            }
        }
        return true;

    }

    private bool IsAllowedToTarget()
    {
        return true;
    }
}