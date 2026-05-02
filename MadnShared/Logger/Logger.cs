namespace MadnShared.Logger;

public class Logger : ILogger
{
    private readonly List<ILogWriter> _writer = new();

    public void AddWriter(ILogWriter writer) => _writer.Add(writer);

    public void LogInfo(string msg) => Log(LogLevel.Info, msg);
    public void LogWarning(string msg) => Log(LogLevel.Warning, msg);
    public void LogError(string msg) => Log(LogLevel.Error, msg);

    private void Log(LogLevel level, string message) 
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        var formatted = $"[{timestamp}] [{level}] {message}";
        
        foreach (var writer in _writer) {
            writer.Write(level, formatted);
        }
    }
}