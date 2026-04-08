using System;
using System.Threading.Tasks;
using MadnShared.Messages.ServerToClient;
using MadnShared.Logger;
using MadnShared.Messages.Base;
using MadnShared.Messages.ClientToServer;

namespace MadnClient
{
    public class GameFrontend
    {
        private readonly IWebSocketClient _wsClient;
        private readonly Guid _playerId;

        public GameFrontend(IWebSocketClient wsClient, Guid playerId)
        {
            _wsClient = wsClient;
            _playerId = playerId;
        }

        public async Task EnterGameAsync(Guid gameId)
        {
            Console.Clear();
            Console.WriteLine($"Spiel: {gameId}");
            Console.WriteLine();
            Console.WriteLine("Optionen:");
            Console.WriteLine("B) Spiel verlassen");
            Console.WriteLine("W) Würfeln");
            Console.WriteLine("A/D) Figur auswählen");
            Console.WriteLine("Enter) Figur bewegen");

            bool stay = true;
            while (stay)
            {
                var key = Console.ReadKey(true);
                switch (key.Key)
                {
                    case ConsoleKey.B:
                        break;
                    case ConsoleKey.W:
                        break;
                    // Arrow Keys or A/D for piece selection
                    case ConsoleKey.A:
                    case ConsoleKey.LeftArrow: 
                        break;
                    case ConsoleKey.D:  
                    case ConsoleKey.RightArrow:
                        break;
                        
                    case ConsoleKey.Enter:
                        
                    default:
                        Console.WriteLine("Unbekannte Option. 'B' zum Zurückkehren.");
                        break;
                }
                await Task.Delay(100);
            }

            Console.Clear();
            Console.WriteLine("Zurück zum Menü...");
            await Task.Delay(300);
        }
        
        private void DrawGameBoard()
        {
            Console.Clear();
        }
    }
}

