using MadnServer.Player;
using MadnShared.Enums;
using MadnShared.Messages.Base;

namespace MadnServerTest.Mocks;

public class MockPlayer : IPlayer
{
    public Guid Id { get; } = Guid.NewGuid();
    public Color Color { get; set; }

    public List<IMessage> SentMessages { get; } = new();

    public MockPlayer()
    {
        
    }

    public Task SendAsync(IMessage message)
    {
        SentMessages.Add(message);
        return Task.CompletedTask;
    }
}
