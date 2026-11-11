namespace AITerminal.Infrastructure;

public sealed class StorageOptions
{
    public StorageOptions(string dataDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dataDirectory);
        DataDirectory = Path.GetFullPath(dataDirectory);
        DatabasePath = Path.Combine(DataDirectory, "ai-terminal.db");
    }

    public string DataDirectory { get; }

    public string DatabasePath { get; }
}
