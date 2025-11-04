using MadnServer.Gamelogic;
using MadnShared.Enums;

namespace MadnServer.Player;

/// <summary>
/// this player tries really hard to kick other players figures out
/// </summary>
public class CpuPlayerHard : ICpuPlayer
{
    public Color Color { get; set; }
}