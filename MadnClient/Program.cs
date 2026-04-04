using MadnShared.GameAssets;
using MadnShared.Logger;
using System.Threading.Tasks;

namespace MadnClient;

public class Program
{
    public static void Main(string[] args)
    {
        Logger.AddWriter(new FileWriter("logs/log.txt"));
        Logger.LogInfo("Starting Client");
        
        new ConsoleClient().RunAsync("ws://localhost:5000/ws").GetAwaiter().GetResult();
    }
}