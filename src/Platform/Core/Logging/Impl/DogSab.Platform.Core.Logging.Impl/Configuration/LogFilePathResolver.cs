namespace DogSab.Platform.Core.Logging.Impl.Configuration;

/// <summary>
/// Resolves the platform-appropriate directory and file path for DogSab's own
/// log files, following each OS's conventional location for application logs
/// rather than hardcoding a single path.
/// </summary>
public static class LogFilePathResolver
{
    /// <summary>The file name used for the current (non-rotated) log file.</summary>
    private const string LogFileName = "dogsab.log";
    
    /// <summary>
    /// Returns the directory DogSab should write its log files into, creating it
    /// if it does not already exist.
    /// </summary>
    /// <returns>The absolute path to the logs directory.</returns>
    public static string GetLogsDirectory()
    {
        var baseDirectory = GetPlatformBaseDirectory();
        var logsDirectory = Path.Combine(baseDirectory, "DogSab", "logs");

        Directory.CreateDirectory(logsDirectory);

        return logsDirectory;
    }

    /// <summary>
    /// Returns the full path to the current log file.
    /// </summary>
    /// <returns>The absolute path to <c>dogsab.log</c> inside the resolved logs directory.</returns>
    public static string GetCurrentLogFilePath()
    {
        return Path.Combine(GetLogsDirectory(), LogFileName);
    }

    /// <summary>
    /// Returns the base directory for application data, chosen per-OS to match
    /// platform conventions: <c>%APPDATA%</c> on Windows, <c>~/.local/share</c> on
    /// Linux, and <c>~/Library/Logs</c> on macOS (which conventionally separates
    /// logs from other application support data).
    /// </summary>
    /// <returns>The absolute path to the OS-appropriate base directory.</returns>
    private static string GetPlatformBaseDirectory()
    {
        if (OperatingSystem.IsWindows())
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        }

        if (OperatingSystem.IsMacOS())
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Combine(home, "Library", "Logs");
        }

        // Linux and other Unix-likes: XDG Base Directory convention.
        var xdgDataHome = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
        if (!string.IsNullOrEmpty(xdgDataHome))
        {
            return xdgDataHome;
        }

        var fallbackHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(fallbackHome, ".local", "share");
    }
}