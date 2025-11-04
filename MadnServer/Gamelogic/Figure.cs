using MadnShared.Enums;

namespace MadnServer.Gamelogic;

public class Figure
{
    public Figure(Color color)
    {
        Color = color;
    }
    
    public Color Color { get; private set; }
}