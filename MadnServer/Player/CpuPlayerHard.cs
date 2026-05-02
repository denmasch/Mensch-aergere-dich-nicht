using System;
using System.Collections.Generic;
using System.Linq;
using MadnServer.Gamelogic;
using MadnShared.GameAssets;

namespace MadnServer.Player;

public class CpuPlayerHard : CpuPlayer
{
    public CpuPlayerHard(Game game) : base(game)
    {
    }
    
    protected override Move SelectMove(IReadOnlyList<Move> validMoves)
    {
        var capture = validMoves.FirstOrDefault(m => m.IsCapture);
        return capture ?? validMoves[Random.Shared.Next(validMoves.Count)];
    }
}