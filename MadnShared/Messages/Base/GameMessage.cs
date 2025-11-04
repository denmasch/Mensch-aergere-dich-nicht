namespace MadnShared.Messages.Base;

public abstract class GameMessage
{
    public string Type { get; protected set; } = "";
}