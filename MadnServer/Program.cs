using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using MadnServer.Gamelogic;
using MadnServer.Player;
using MadnServer.Services;
using MadnShared.Logger;
using MadnShared.Messages.Base;
using MadnShared.Utils;
using MadnShared.Messages.ServerToClient;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace MadnServer;

class Program
{
    static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        var logger = new Logger();
        logger.AddWriter(new ConsoleWriter());
        logger.AddWriter(new FileWriter("logs/ServerLog.txt"));
        builder.Services.AddSingleton<ILogger>(logger);
        
        builder.Services.AddSingleton<IStatsService, StatsService>();
        builder.Services.AddSingleton<IGameManager, GameManager>();
        builder.Services.AddSingleton<IMessageDispatcher, MessageDispatcher>();

        
        var app = builder.Build();
        
        app.UseWebSockets();

        app.Map("/ws", async (
            HttpContext context,
            ILogger logger,
            IStatsService statsService,
            IMessageDispatcher messageDispatcher,
            IGameManager gameManager
            ) =>
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                logger.LogInfo("Client connected");
                
                IPlayer player = new RealPlayer(webSocket);

                try
                {
                    var welcome = new WelcomeMessage { ClientId = player.Id };
                    await player.SendAsync(welcome);
                    logger.LogInfo($"Sent WelcomeMessage to client {player.Id}");
                }
                catch (Exception ex)
                {
                    logger.LogError($"Failed to send WelcomeMessage to client {player.Id}: {ex.Message}");
                }

                var buffer = new byte[1024 * 4];
                using var ms = new MemoryStream();
                try
                {
                    while (webSocket.State == WebSocketState.Open)
                    {
                        WebSocketReceiveResult result;
                        ms.SetLength(0);
                        ms.Seek(0, SeekOrigin.Begin);
                        do
                        {
                            result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer),
                                CancellationToken.None);
                            if (result.MessageType == WebSocketMessageType.Close)
                            {
                                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed connection",
                                    CancellationToken.None);
                                logger.LogInfo("Client disconnected");
                                break;
                            }

                            ms.Write(buffer, 0, result.Count);
                        } while (!result.EndOfMessage);

                        ms.Seek(0, SeekOrigin.Begin);
                        using var reader = new StreamReader(ms, Encoding.UTF8, leaveOpen: true);
                        var msgJson = await reader.ReadToEndAsync();

                        var msg = MessageSerializer.Deserialize(msgJson);
                        logger.LogInfo("Received Message: " + msgJson);
                        if (msg is null)
                            continue;

                        await messageDispatcher.DispatchAsync(player, msg);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError("WebSocket error: " + ex.Message);
                }
                finally
                {
                    gameManager.RemovePlayerFromGames(player);
                    logger.LogInfo($"Cleaned up player {player.Id} on disconnect.");
                }
            }
            else
            {
                context.Response.StatusCode = 400;
            }
        });

        logger.LogInfo("Server started");
        app.Run("http://0.0.0.0:5000");
        logger.LogInfo("Server stopped");
    }
}
