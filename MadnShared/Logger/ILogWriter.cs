namespace MadnShared.Logger;

public interface ILogWriter
{
    void Write(LogLevel level, string message);
}