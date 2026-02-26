namespace MadnShared.GameAssets;
using MadnShared.Enums;
public class MoveValidator
{
    public bool validateMove(GameboardState state, Figure fig, int diceRollCount )
    {
        bool isAllowed = false;
        
        //TODO: Test if Movement is allowed
        
        /* Allowed Movement:
        - Figure moves from Tile of Path to empty Tile of
            - Path
            - own-color Targets 
                - if diceRollCount does not exceed Targets
        - Figure captures enemy Figure
        - Figures gets out of Home Tiles to Path 
            - only if Path Tile
                - is empty
                - is enemy figures -> gets captured
        */

        return isAllowed;
    }
    
}