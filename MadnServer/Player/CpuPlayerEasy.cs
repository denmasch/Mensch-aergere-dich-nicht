using System;
using System.Collections.Generic;
using System.Linq;
using MadnShared.GameAssets;

namespace MadnServer.Player;

public class CpuPlayerEasy : CpuPlayer
{
    protected override Move SelectMove(IReadOnlyList<Move> validMoves)
    {
        var nonCapture = validMoves.FirstOrDefault(m => !m.IsCapture);
        return nonCapture ?? validMoves[Random.Shared.Next(validMoves.Count)];
    }
}