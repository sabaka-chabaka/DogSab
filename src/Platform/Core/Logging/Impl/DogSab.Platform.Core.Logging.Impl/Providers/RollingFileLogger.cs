using System.Text;
using Microsoft.Extensions.Logging;

namespace DogSab.Platform.Core.Logging.Impl.Providers;

/// <summary>
/// A Microsoft.Extensions.Logging <see cref="ILogger"/> that appends formatted
/// log lines to a shared, size-rotated file. All logger instances for a given
/// category funnel through the same <see cref="RollingFileWriter"/> owned by
/// the parent <see cref="RollingFileLoggerProvider"/>, since rotation must be
/// coordinated across every category writing to the same physical file.
/// </summary>
internal sealed class RollingFileLogger : ILogger
{
    /// <summary>The category name this logger reports messages under.</summary>
    private readonly string _categoryName;

    /// <summary>The shared writer that performs the actual file I/O and rotation.</summary>
    private readonly RollingFileWriter _writer;

    /// <summary>The minimum level a message must have to be written.</summary>
    private readonly LogLevel _minimumLevel;

    /// <summary>
    /// Creates a new rolling file logger for a specific category.
    /// </summary>
    /// <param name="categoryName">The category name this logger reports messages under.</param>
    /// <param name="writer">The shared file writer to append formatted lines to.</param>
    /// <param name="minimumLevel">The minimum level a message must have to be written.</param>
    public RollingFileLogger(string categoryName, RollingFileWriter writer, LogLevel minimumLevel)
    {
        _categoryName = categoryName;
        _writer = writer;
        _minimumLevel = minimumLevel;
    }

    /// <summary>
    /// Rolling file output does not support structured logging scopes; returns
    /// a no-op disposable so callers using <c>BeginScope</c> do not fail.
    /// </summary>
    /// <typeparam name="TState">The scope state type.</typeparam>
    /// <param name="state">The scope state value.</param>
    /// <returns>A no-op disposable.</returns>
    public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

    /// <summary>
    /// Checks whether the given level meets this logger's configured minimum level.
    /// </summary>
    /// <param name="logLevel">The level to check.</param>
    /// <returns><c>true</c> if messages at this level are written; otherwise <c>false</c>.</returns>
    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None && logLevel >= _minimumLevel;

    /// <summary>
    /// Formats and appends a log entry to the shared rolling file writer, if its
    /// level meets the configured minimum.
    /// </summary>
    /// <typeparam name="TState">The state type associated with the log entry.</typeparam>
    /// <param name="logLevel">The severity of the entry.</param>
    /// <param name="eventId">The event ID associated with the entry.</param>
    /// <param name="state">The entry's state, formatted via <paramref name="formatter"/>.</param>
    /// <param name="exception">An exception associated with the entry, if any.</param>
    /// <param name="formatter">Function producing the final message text from <paramref name="state"/> and <paramref name="exception"/>.</param>
    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        var line = FormatLine(logLevel, state, exception, formatter);
        _writer.AppendLine(line);
    }

    /// <summary>
    /// Builds a single formatted log line: timestamp, level, category, message,
    /// and (if present) the exception's full details on a following indented block.
    /// </summary>
    /// <typeparam name="TState">The state type associated with the log entry.</typeparam>
    /// <param name="logLevel">The severity of the entry.</param>
    /// <param name="state">The entry's state.</param>
    /// <param name="exception">An exception associated with the entry, if any.</param>
    /// <param name="formatter">Function producing the final message text.</param>
    /// <returns>The formatted line, including a trailing exception block if applicable.</returns>
    private string FormatLine<TState>(
        LogLevel logLevel,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        var builder = new StringBuilder();
        builder.Append('[').Append(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff")).Append("] ");
        builder.Append('[').Append(LevelLabel(logLevel)).Append("] ");
        builder.Append('[').Append(_categoryName).Append("] ");
        builder.Append(formatter(state, exception));

        if (exception is not null)
        {
            builder.Append(Environment.NewLine).Append(exception);
        }

        return builder.ToString();
    }

    /// <summary>
    /// Returns a fixed-width, human-readable label for a log level, for consistent
    /// column alignment in the file output.
    /// </summary>
    /// <param name="logLevel">The level to label.</param>
    /// <returns>A short uppercase label for the level.</returns>
    private static string LevelLabel(LogLevel logLevel) => logLevel switch
    {
        LogLevel.Trace => "TRACE",
        LogLevel.Debug => "DEBUG",
        LogLevel.Information => "INFO ",
        LogLevel.Warning => "WARN ",
        LogLevel.Error => "ERROR",
        LogLevel.Critical => "FATAL",
        _ => "?????"
    };

    /// <summary>A no-op <see cref="IDisposable"/> returned from <see cref="BeginScope{TState}"/>, since scopes are not supported.</summary>
    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();
        public void Dispose() { }
    }
}