using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MadnShared.Logger;
using MadnShared.Messages.Base;
using MadnShared.Utils;

namespace MadnClient;

public class WebSocketClient : IWebSocketClient, IDisposable
{
    private ClientWebSocket? _socket;
    private CancellationTokenSource _cts = new();
    public event Action<IMessage> MessageReceived = delegate { };

    public bool IsConnected => _socket != null && _socket.State == WebSocketState.Open;

    public async Task ConnectAsync(string serverUri)
    {
        if (IsConnected) return;

        _socket = new ClientWebSocket();
        try
        {
            Logger.LogInfo("Connecting to " + serverUri);
            await _socket.ConnectAsync(new Uri(serverUri), CancellationToken.None);
            Logger.LogInfo("Connected to server at " + serverUri);
            _cts = new CancellationTokenSource();
            _ = Task.Run(() => ReceiveLoopAsync(_socket, _cts.Token));
        }
        catch (Exception ex)
        {
            Logger.LogError("WebSocket connect failed: " + ex.Message);
            _socket?.Dispose();
            _socket = null;
        }
    }

    public async Task SendAsync(IMessage message)
    {
        if (_socket == null || _socket.State != WebSocketState.Open)
        {
            Logger.LogError("Cannot send message, not connected to server.");
            return;
        }
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

                await _socket.SendAsync(
                    new ArraySegment<byte>(buffer, 0, bytesRead),
                    WebSocketMessageType.Text,
                    isLastPart,
                    CancellationToken.None
                );
            }
        }
    }

    private async Task ReceiveLoopAsync(ClientWebSocket socket, CancellationToken ct)
    {
        var buffer = new byte[4096];
        using var ms = new MemoryStream();
        try
        {
            while (!ct.IsCancellationRequested && socket.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result;
                ms.SetLength(0); 
                ms.Seek(0, SeekOrigin.Begin);

                do
                {
                    result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), ct);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        Logger.LogInfo("Server closed");
                        await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closing",
                            CancellationToken.None);
                        break;
                    }
                    ms.Write(buffer, 0, result.Count);
                }
                while (!result.EndOfMessage);

                ms.Seek(0, SeekOrigin.Begin);
                using var reader = new StreamReader(ms, Encoding.UTF8, leaveOpen: true);
                var msgJson = await reader.ReadToEndAsync();
                
                var gameMsg = MessageSerializer.Deserialize(msgJson);
                
                try
                {
                    MessageReceived?.Invoke(gameMsg);
                }
                catch (Exception ex)
                {
                    Logger.LogError("Error in MessageReceived handler: " + ex.Message);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError("ReceiveLoop error: " + ex.Message);
        }
    }

    public async Task CloseAsync()
    {
        try
        {
            _cts.Cancel();
            if (_socket != null)
            {
                if (_socket.State == WebSocketState.Open)
                {
                    await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closing", CancellationToken.None);
                }
                _socket.Dispose();
                _socket = null;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError("Error closing WebSocket: " + ex.Message);
        }
    }

    public void Dispose()
    {
        try
        {
            _cts.Cancel();
            _socket?.Dispose();
        }
        catch { }
    }
}

