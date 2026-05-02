using System.Text;
using MadnShared.GameAssets;
using MadnShared.Logger;
using System.Threading.Tasks;

namespace MadnClient;

public class Program
{
    public static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;
        
        var logger = new Logger();
        logger.AddWriter(new FileWriter("logs/ClientLog.txt"));
        logger.LogInfo("Starting Client");
        
        var wsClient = new WebSocketClient(logger);
        var consoleClient = new ConsoleClient(wsClient, logger);
        consoleClient.RunAsync("ws://localhost:5000/ws").GetAwaiter().GetResult();
    }
}