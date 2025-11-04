using MadnServer.Gamelogic;
using MadnShared.Enums;

namespace MadnServer.Player;

/// <summary>
/// this player tries to advance its figures and occasionally kicks out other players figures
/// </summary>
public class CpuPlayerMedium : ICpuPlayer
{
    public Color Color { get; set; }
}