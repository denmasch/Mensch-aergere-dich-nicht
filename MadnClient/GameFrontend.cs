using System;
using System.Threading.Tasks;
using MadnShared.Messages.ServerToClient;
using MadnShared.Logger;
using MadnShared.Messages.Base;
using MadnShared.Messages.ClientToServer;
using MadnShared.Enums;
using MadnShared.GameAssets;

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
        
        private void DrawGameBoard(GameboardDTO board)
        {
            Console.Clear();
            
            for (int y = 0; y < 11; y++)
            {
                for (int x = 0; x < 11; x++)
                {
                    DrawTileAt(x, y, board);
                }
                Console.ResetColor();
                Console.WriteLine();
            }
        }

        private void DrawTileAt(int x, int y, GameboardDTO board)
        {
            ConsoleColor bg = ConsoleColor.Black;
            ConsoleColor fg = ConsoleColor.White;
            string symbol = "  ";

            // The Homes 
            if (x <= 1 && y <= 1) { bg = ConsoleColor.Yellow; fg = ConsoleColor.DarkYellow; } // top left
            else if (x >= 9 && y <= 1) { bg = ConsoleColor.Green; fg = ConsoleColor.DarkGreen; } // top right
            else if (x <= 1 && y >= 9) { bg = ConsoleColor.Blue; fg = ConsoleColor.DarkBlue; } // bottom left
            else if (x >= 9 && y >= 9) { bg = ConsoleColor.Red; fg = ConsoleColor.DarkRed; } // bottom right
            
            // The Path
            else if ((x >= 4 && x <= 6) || (y >= 4 && y <= 6)) 
            { 
                bg = ConsoleColor.Gray; 
                fg = ConsoleColor.DarkGray; 
            }
            
            // Get Target Tile
            if (x == 5 && y == 5) { bg = ConsoleColor.Black; fg = ConsoleColor.White; }
            else if (x >= 1 && x <= 5 && y == 5) { bg = ConsoleColor.Yellow; fg = ConsoleColor.DarkYellow; }
            else if (x == 5 && y >= 1 && y <= 5 ) { bg = ConsoleColor.Green; fg = ConsoleColor.DarkGreen; }
            else if (x >= 5 && x <= 9 && y == 5) { bg = ConsoleColor.Red; fg = ConsoleColor.Red; }
            else if (x == 5 && y >= 5 && y <= 9) { bg = ConsoleColor.Blue; fg = ConsoleColor.DarkBlue; }

            // TODO: get tile from board and check if occupied
            // if (tile != null && tile.IsOccupied) 
            // { 
            //     symbol = "♙ "; 
            //     fg = GetFigureColor(tile.OccupyingFigure.Color);
            // }

            if (bg != ConsoleColor.Black) 
            {
                Console.BackgroundColor = bg;
                Console.ForegroundColor = fg;
                Console.Write(symbol);
            }
            else
            {
                Console.ResetColor();
                Console.Write("  ");
            }
            Console.ResetColor();
            Console.Write(" ");
        }
        
        private ConsoleColor GetFigureColor(Color color)
        {
            return color switch
            {
                Color.Yellow => ConsoleColor.DarkYellow,
                Color.Green => ConsoleColor.DarkGreen,
                Color.Blue => ConsoleColor.DarkBlue,
                Color.Red => ConsoleColor.DarkRed,
                _ => ConsoleColor.Black
            };
        }
    }
}
