using System.Security.Cryptography;
namespace MadnServer.Gamelogic;

public static class Dice
{
    public static int RollDice()
    {
        return RandomNumberGenerator.GetInt32(1, 7);
    }
}