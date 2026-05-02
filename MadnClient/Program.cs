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
        
        while (true)
        {
            var serverUri = AskServerUri();
            try
            {
                consoleClient.RunAsync(serverUri).GetAwaiter().GetResult();
                break;
            }
            catch (Exception ex)
            {
                Console.Clear();
                Console.WriteLine("Verbindung fehlgeschlagen: " + ex.Message);
                Console.WriteLine("Bitte überprüfen Sie die Serveradresse und stellen Sie sicher, dass der Server läuft.");
                Console.WriteLine();
                Console.WriteLine("Drücken Sie eine beliebige Taste.");
                Console.ReadKey(true);
                continue;
            }
        }
    }

    private static string AskServerUri()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("┌─────────────────────────┐");
            Console.WriteLine("│ Server verbinden        │");
            Console.WriteLine("├─────────────────────────┤");
            Console.WriteLine("│ 1) localhost            │");
            Console.WriteLine("│ 2) Eigene IP / Hostname │");
            Console.WriteLine("└─────────────────────────┘");
            Console.Write("Auswahl: ");

            var key = Console.ReadKey(true).KeyChar;
            Console.WriteLine(key);

            switch (key)
            {
                case '1':
                    return "ws://localhost:5000/ws";

                case '2':
                    return AskCustomServerUri();

                default:
                    Console.WriteLine("Ungültige Auswahl. Bitte erneut versuchen.");
                    Console.ReadKey(true);
                    break;
            }
        }
        
        
    }
    
    private static string AskCustomServerUri()
    {
        while (true)
        {
            Console.Write("Bitte geben Sie die IP-Adresse oder den Hostnamen ein: ");
            string input = Console.ReadLine()?.Trim();

            if (!string.IsNullOrEmpty(input) && Uri.CheckHostName(input) != UriHostNameType.Unknown)
            {
                return $"ws://{input}:5000/ws";
            }

            Console.WriteLine("Ungültige Eingabe. Bitte geben Sie einen validen Hostnamen oder eine IP-Adresse ein.");
        }
    }
}
