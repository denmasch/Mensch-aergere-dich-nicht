using MadnShared.Messages.Base;

namespace MadnShared.Messages.ServerToClient;

public class WelcomeMessage : IMessage
{
    public string Type => MessageType.Welcome;
    
    public Guid ClientId { get; set; }
}