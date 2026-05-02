namespace MadnShared.Logger;

public interface ILogger
{
    public void AddWriter(ILogWriter writer);
    public void LogInfo(string msg);
    public void LogWarning(string msg);
    public void LogError(string msg);
}