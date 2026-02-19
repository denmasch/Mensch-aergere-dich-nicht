using MadnShared.GameAssets;

namespace MadnClient;

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Hello, MadnClient!");
        
        Gameboard board = new Gameboard();

        // Just for Testing purposes:
        board.DrawBoard();
    }
}