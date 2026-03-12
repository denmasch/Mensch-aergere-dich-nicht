namespace MadnShared.Messages.Base;

public interface IGameMessage
{
    public string Type { get; }
    
    public Guid GameId { get; set; }
}