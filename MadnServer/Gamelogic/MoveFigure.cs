namespace MadnServer.Gamelogic;
using MadnShared.Enums;

public class MoveFigure
{
    private MoveValidator _validator;

    public void Move(Gameboard gb, Figure fig, Color activePlayer, int diceRollCount )
    {
        _validator = new MoveValidator();
        
        bool movementAllowed = _validator.ValidateMove(gb, fig, activePlayer, diceRollCount);
        if (!movementAllowed)
        {
           // TODO: Add Logic for Moving player in GameState with given Gameboard rules
           
           //Move Captured Figure back to Home
        }
        
    }
    
}