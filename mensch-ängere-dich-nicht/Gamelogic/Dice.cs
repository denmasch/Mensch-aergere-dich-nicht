using System;

namespace mensch_ängere_dich_nicht.Gamelogic;

public static class Dice
{
    public static int RollDice()
    {
        return Random.Shared.Next(1, 7);
    }
}