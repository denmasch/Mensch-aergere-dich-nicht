using System;
using System.IO;
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
        Logger.AddWriter(new FileWriter("logs/ServerLog.txt"));
        
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
                using var ms = new MemoryStream();
                while (webSocket.State == WebSocketState.Open)
                {
                    WebSocketReceiveResult result;
                    ms.SetLength(0); 
                    ms.Seek(0, SeekOrigin.Begin);
                    do
                    {
                        result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed connection",
                                CancellationToken.None);
                            Logger.LogInfo("Client disconnected");
                            break;
                        }
                        ms.Write(buffer, 0, result.Count);
                    }
                    while (!result.EndOfMessage);

                    ms.Seek(0, SeekOrigin.Begin);
                    using var reader = new StreamReader(ms, Encoding.UTF8, leaveOpen: true);
                    var msgJson = await reader.ReadToEndAsync();
                    
                    var msg = MessageSerializer.Deserialize(msgJson);
                    Logger.LogInfo("Received Message: "+ msgJson);
                    if (msg is null)
                        continue;

                    MessageDispatcher.Dispatch(player, msg);
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
