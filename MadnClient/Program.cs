using MadnShared.GameAssets;
using MadnShared.Logger;
using System.Threading.Tasks;

namespace MadnClient;

public class Program
{
    public static void Main(string[] args)
    {
        Logger.AddWriter(new FileWriter("logs/ClientLog.txt"));
        Logger.LogInfo("Starting Client");
        
        var wsClient = new WebSocketClient();
        var consoleClient = new ConsoleClient(wsClient);
        consoleClient.RunAsync("ws://localhost:5000/ws").GetAwaiter().GetResult();
    }
}