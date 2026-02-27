using System;
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

    public async Task SendAsync(IGameMessage message)
    {
        if (_webSocket == null)
            return;

        if (_webSocket.State != WebSocketState.Open)
            return;

        var json = MessageSerializer.Serialize(message);
        var buffer = Encoding.UTF8.GetBytes(json);

        await _webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, endOfMessage: true, cancellationToken: CancellationToken.None);
    }
}