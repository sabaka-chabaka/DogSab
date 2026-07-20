using DogSabLogLevel = DogSab.Platform.Core.Abstractions.Logging.LogLevel;

namespace DogSab.Platform.Core.Logging.Impl.Configuration;

/// <summary>
/// Configuration governing how the platform logs: minimum level, whether console
/// output is enabled, and file rotation limits. Currently sourced from environment
/// variables and hardcoded defaults rather than the platform's persistent settings
/// system, to avoid an initialization-order dependency on <c>Core.Settings.Impl</c>
/// during the earliest phase of application bootstrap, before that subsystem exists.
/// If persistent, user-editable logging settings are needed later, this type can be
/// re-implemented as an <c>IPersistentStateComponent&lt;LoggingOptions&gt;</c> without
/// changing how the rest of the logging module consumes it.
/// </summary>
public sealed class LoggingOptions
{
    /// <summary>Name of the environment variable used to override the minimum log level.</summary>
    private const string MinimumLevelEnvVar = "DOGSAB_LOG_LEVEL";

    /// <summary>Name of the environment variable used to disable console logging.</summary>
    private const string DisableConsoleEnvVar = "DOGSAB_LOG_NO_CONSOLE";

    /// <summary>Name of the environment variable used to override the maximum log file size before rotation.</summary>
    private const string MaxFileSizeBytesEnvVar = "DOGSAB_LOG_MAX_FILE_SIZE_BYTES";

    /// <summary>Name of the environment variable used to override how many rotated log files are retained.</summary>
    private const string RetainedFileCountEnvVar = "DOGSAB_LOG_RETAINED_FILES";

    /// <summary>The minimum level a message must have to be recorded by any provider.</summary>
    public DogSabLogLevel MinimumLevel { get; init; } = DogSabLogLevel.Info;

    /// <summary>Whether log messages are also written to the console. Typically enabled only in development.</summary>
    public bool ConsoleEnabled { get; init; }

    /// <summary>The maximum size, in bytes, a log file may reach before it is rotated.</summary>
    public long MaxFileSizeBytes { get; init; } = 10 * 1024 * 1024; // 10 MB

    /// <summary>How many rotated log files are kept before the oldest is deleted.</summary>
    public int RetainedFileCount { get; init; } = 5;
    
    /// <summary>
    /// Builds a <see cref="LoggingOptions"/> instance from environment variables,
    /// falling back to sensible defaults for any variable that is unset or invalid.
    /// </summary>
    /// <returns>The resolved logging options.</returns>
    public static LoggingOptions FromEnvironment()
    {
        return new LoggingOptions
        {
            MinimumLevel = ParseMinimumLevel(Environment.GetEnvironmentVariable(MinimumLevelEnvVar)),
            ConsoleEnabled = !IsTruthy(Environment.GetEnvironmentVariable(DisableConsoleEnvVar)),
            MaxFileSizeBytes = ParsePositiveLong(
                Environment.GetEnvironmentVariable(MaxFileSizeBytesEnvVar),
                defaultValue: 10 * 1024 * 1024),
            RetainedFileCount = ParsePositiveInt(
                Environment.GetEnvironmentVariable(RetainedFileCountEnvVar),
                defaultValue: 5)
        };
    }

    /// <summary>
    /// Parses a minimum log level from its string name, falling back to
    /// <see cref="DogSabLogLevel.Info"/> if the value is missing or unrecognized.
    /// </summary>
    /// <param name="rawValue">The raw environment variable value, or <c>null</c>.</param>
    /// <returns>The parsed level, or the default if parsing failed.</returns>
    private static DogSabLogLevel ParseMinimumLevel(string? rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return DogSabLogLevel.Info;
        }

        return Enum.TryParse<DogSabLogLevel>(rawValue, ignoreCase: true, out var parsed)
            ? parsed
            : DogSabLogLevel.Info;
    }

    /// <summary>
    /// Interprets common truthy string values ("1", "true", "yes") case-insensitively.
    /// </summary>
    /// <param name="rawValue">The raw environment variable value, or <c>null</c>.</param>
    /// <returns><c>true</c> if the value represents an affirmative flag; otherwise <c>false</c>.</returns>
    private static bool IsTruthy(string? rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return false;
        }

        return rawValue.Equals("1", StringComparison.OrdinalIgnoreCase)
            || rawValue.Equals("true", StringComparison.OrdinalIgnoreCase)
            || rawValue.Equals("yes", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Parses a positive <see cref="long"/> from an environment variable, falling
    /// back to <paramref name="defaultValue"/> if missing, invalid, or non-positive.
    /// </summary>
    /// <param name="rawValue">The raw environment variable value, or <c>null</c>.</param>
    /// <param name="defaultValue">The value to use if parsing fails.</param>
    /// <returns>The parsed value, or the default.</returns>
    private static long ParsePositiveLong(string? rawValue, long defaultValue)
    {
        return long.TryParse(rawValue, out var parsed) && parsed > 0 ? parsed : defaultValue;
    }

    /// <summary>
    /// Parses a positive <see cref="int"/> from an environment variable, falling
    /// back to <paramref name="defaultValue"/> if missing, invalid, or non-positive.
    /// </summary>
    /// <param name="rawValue">The raw environment variable value, or <c>null</c>.</param>
    /// <param name="defaultValue">The value to use if parsing fails.</param>
    /// <returns>The parsed value, or the default.</returns>
    private static int ParsePositiveInt(string? rawValue, int defaultValue)
    {
        return int.TryParse(rawValue, out var parsed) && parsed > 0 ? parsed : defaultValue;
    }
}