using mensch_ängere_dich_nicht.Gamelogic;

namespace mensch_ängere_dich_nicht.Player;

/// <summary>
/// this player tries to advance its figures and occasionally kicks out other players figures
/// </summary>
public class CpuPlayerMedium : ICpuPlayer
{
    public Color Color { get; set; }
}