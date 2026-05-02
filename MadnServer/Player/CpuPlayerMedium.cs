using System;
using System.Collections.Generic;
using MadnShared.GameAssets;

namespace MadnServer.Player;

public class CpuPlayerMedium : CpuPlayer
{
    protected override Move SelectMove(IReadOnlyList<Move> validMoves)
    {
        return validMoves[Random.Shared.Next(validMoves.Count)];
    }
}