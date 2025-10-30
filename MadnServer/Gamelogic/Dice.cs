using System;

namespace MadnServer.Gamelogic;

public static class Dice
{
    public static int RollDice()
    {
        return Random.Shared.Next(1, 7);
    }
}