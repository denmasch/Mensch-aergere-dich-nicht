namespace MadnShared.Messages.Base;

public interface IGameMessage : IMessage
{
    public Guid GameId { get; set; }
}