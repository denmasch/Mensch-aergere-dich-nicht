using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MadnServer.Gamelogic;
using MadnShared.Enums;
using MadnShared.Messages.Base;
using MadnShared.Utils;

namespace MadnServer.Player;

public class RealPlayer : IPlayer
{
    public RealPlayer(WebSocket webSocket)
    {
        _webSocket = webSocket;
    }
    public Guid Id { get; } = Guid.NewGuid();
    
    private WebSocket _webSocket;
    
    public Color Color { get; set; }

    public async Task SendAsync(IMessage message)
    {
        if (_webSocket == null)
            return;

        if (_webSocket.State != WebSocketState.Open)
            return;

        var json = MessageSerializer.Serialize(message);
        var bytes = Encoding.UTF8.GetBytes(json);
        using (var ms = new MemoryStream(bytes))
        {
            int bufferSize = 1024 * 4; 
            byte[] buffer = new byte[bufferSize];
            int bytesRead;

            while ((bytesRead = ms.Read(buffer, 0, bufferSize)) > 0)
            {
                bool isLastPart = (ms.Position == ms.Length);
            
                await _webSocket.SendAsync(
                    new ArraySegment<byte>(buffer, 0, bytesRead),
                    WebSocketMessageType.Text,
                    isLastPart, 
                    CancellationToken.None
                );
            }
        }
    }
}