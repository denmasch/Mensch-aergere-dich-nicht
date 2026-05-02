using System;
using System.Collections.Generic;
using MadnServer.Gamelogic;
using MadnShared.GameAssets;

namespace MadnServer.Player;

public class CpuPlayerMedium : CpuPlayer
{
    public CpuPlayerMedium(Game game) : base(game)
    {
    }
    
    protected override Move SelectMove(IReadOnlyList<Move> validMoves)
    {
        return validMoves[Random.Shared.Next(validMoves.Count)];
    }
}