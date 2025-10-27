using mensch_ängere_dich_nicht.Gamelogic;

namespace mensch_ängere_dich_nicht.Player;

/// <summary>
/// this player has no stategy and plays random moves
/// </summary>
public class CpuPlayerEasy : ICpuPlayer
{
    public Color Color { get; set; }
}