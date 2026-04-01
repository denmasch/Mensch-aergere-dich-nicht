using MadnShared.GameAssets;
using MadnShared.Logger;

namespace MadnClient;

public class Program
{
    public static void Main(string[] args)
    {
        Logger.AddWriter(new FileWriter("log.txt"));
        Logger.LogInfo("Starting Client");
    }
}