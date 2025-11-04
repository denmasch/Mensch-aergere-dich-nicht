namespace MadnServer.Gamelogic;

public static class GameManager
{
    public static Game CreateGame()
    {
        return new Game(null);
    }
}