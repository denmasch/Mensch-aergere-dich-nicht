namespace MadnShared.Logger;

public class ConsoleWriter : ILogWriter
{
    public void Write(LogLevel level, string message) 
    {
        switch (level)
        {
            case LogLevel.Warning:
                Console.ForegroundColor = ConsoleColor.Yellow;
                break;
            case LogLevel.Error:
                Console.ForegroundColor = ConsoleColor.Red;
                break;
        }
        Console.WriteLine(message);
        Console.ResetColor();
    }
}