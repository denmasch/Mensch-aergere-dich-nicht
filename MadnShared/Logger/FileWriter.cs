namespace MadnShared.Logger;

public class FileWriter : ILogWriter
{
    private static readonly object _fileLock = new object();
    private readonly string _path;

    public FileWriter(string path)
    {
        _path = path;
        
        string directory = Path.GetDirectoryName(_path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    public void Write(LogLevel level, string message) 
    {
        try
        {
            lock (_fileLock)
            {
                File.AppendAllText(_path, message + Environment.NewLine);
            }
        }
        catch (IOException ex)
        {
            Console.WriteLine($"IOException while writing log file: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"UnauthorizedAccessException while writing log file: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception while writing log file: {ex.Message}");
        }
    }
}