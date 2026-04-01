using MadnShared.Enums;

namespace MadnServer.Gamelogic;

public class Figure
{
    public Figure(Color color, int id)
    {
        Color = color;
        Id = id;
        IsHome = true;
    }
    
    public Color Color { get; private set; }
    
    public int Id { get; private set; }
    
    public bool IsHome { get; set; }
}