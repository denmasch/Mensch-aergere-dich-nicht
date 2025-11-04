using MadnServer.Gamelogic;
using MadnShared.Enums;

namespace MadnServer.Player;

/// <summary>
/// this player has no stategy and plays random moves
/// </summary>
public class CpuPlayerEasy : ICpuPlayer
{
    public Color Color { get; set; }
}