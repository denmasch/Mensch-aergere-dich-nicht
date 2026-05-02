using System;
using System.Collections.Generic;
using System.Linq;
using MadnServer.Gamelogic;
using MadnShared.GameAssets;

namespace MadnServer.Player;

public class CpuPlayerEasy : CpuPlayer
{
    public CpuPlayerEasy(Game game) : base(game)
    {
    }
    
    protected override Move SelectMove(IReadOnlyList<Move> validMoves)
    {
        var nonCapture = validMoves.FirstOrDefault(m => !m.IsCapture);
        return nonCapture ?? validMoves[Random.Shared.Next(validMoves.Count)];
    }
}