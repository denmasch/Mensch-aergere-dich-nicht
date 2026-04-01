using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using MadnServer.Gamelogic;
using MadnServer.Player;
using MadnShared.Logger;
using MadnShared.Messages.Base;
using MadnShared.Utils;

namespace MadnServer;

class Program
{
    static void Main(string[] args)
    {
        Logger.AddWriter(new ConsoleWriter());
        Logger.AddWriter(new FileWriter("logs/log.txt"));
        
        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();
        
        app.UseWebSockets();

        app.Map("/ws", async context =>
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                Logger.LogInfo("Client connected");
                
                IPlayer player = new RealPlayer(webSocket);

                var buffer = new byte[1024 * 4];
                while (webSocket.State == WebSocketState.Open)
                {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed connection", CancellationToken.None);
                        Logger.LogInfo("Client disconnected");
                        break;
                    }

                    var msgJson = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    var gameMsg = MessageSerializer.Deserialize(msgJson);
                    if (gameMsg is null)
                        continue;
                    
                    Guid gameId = gameMsg.GameId;
                    var game = GameManager.GetGame(gameId);
                    if (game != null)
                    {
                        game.HandleMessage(player, gameMsg);
                    }
                }
            }
            else
            {
                context.Response.StatusCode = 400;
            }
        });

        Logger.LogInfo("Server started");
        app.Run("http://0.0.0.0:5000");
        Logger.LogInfo("Server stopped");
    }
}
