using MadnShared.Logger;

namespace MadnServerTest.Mocks;

public class MockLogger : ILogger
{
    private readonly object _lock = new();
    public List<string> InfoMessages { get; } = new();
    public List<string> WarningMessages { get; } = new();
    public List<string> ErrorMessages { get; } = new();

    public void AddWriter(ILogWriter writer)
    {
    }

    public void LogInfo(string msg)
    {
        lock (_lock) { InfoMessages.Add(msg); }
    }

    public void LogWarning(string msg)
    {
        lock (_lock) { WarningMessages.Add(msg); }
    }

    public void LogError(string msg)
    {
        lock (_lock) { ErrorMessages.Add(msg); }
    }
}