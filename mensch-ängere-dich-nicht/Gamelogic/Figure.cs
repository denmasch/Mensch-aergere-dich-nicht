using System.Reflection.Emit;

namespace mensch_ängere_dich_nicht.Gamelogic;

public class Figure
{
    public Figure(Color color)
    {
        Color = color;
    }
    
    public Color Color { get; private set; }
}