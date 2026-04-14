namespace MadnShared.Logger;

public static class Logger 
{
    private static readonly List<ILogWriter> _writer = new();

    public static void AddWriter(ILogWriter writer) => _writer.Add(writer);

    public static void LogInfo(string msg) => Log(LogLevel.Info, msg);
    public static void LogWarning(string msg) => Log(LogLevel.Warning, msg);
    public static void LogError(string msg) => Log(LogLevel.Error, msg);

    private static void Log(LogLevel level, string message) 
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        var formatted = $"[{timestamp}] [{level}] {message}";
        
        foreach (var writer in _writer) {
            writer.Write(level, formatted);
        }
    }
}