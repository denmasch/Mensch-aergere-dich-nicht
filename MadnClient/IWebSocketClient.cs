using MadnShared.Messages.Base;

namespace MadnClient;

public interface IWebSocketClient
{
    event Action<IMessage> MessageReceived;
    Task ConnectAsync(string serverUri);
    Task SendAsync(IMessage message);
    Task CloseAsync();
    bool IsConnected { get; }
}