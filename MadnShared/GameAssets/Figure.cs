using MadnShared.Enums;

namespace MadnShared.GameAssets;

public class Figure
{
    public Figure(Color color, int id)
    {
        Color = color;
        Id = id;
    }
    
    public Color Color { get; private set; }
    
    public int Id { get; private set; }
}