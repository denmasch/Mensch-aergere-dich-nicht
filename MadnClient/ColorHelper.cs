using MadnShared.Enums;

namespace MadnClient;

public static class ColorHelper
{
    public static string ColorToString(Color color)
    {
        switch (color)
        {
            case Color.Yellow: return "Gelb";
            case Color.Green: return "Grün";
            case Color.Blue: return "Blau";
            case Color.Red: return "Rot";
        }
        return "";
    }
}