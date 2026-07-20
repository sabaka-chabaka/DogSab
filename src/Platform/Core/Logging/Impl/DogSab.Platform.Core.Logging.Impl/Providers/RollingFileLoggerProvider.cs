using System.Collections.Concurrent;
using DogSab.Platform.Core.Logging.Impl.Configuration;
using Microsoft.Extensions.Logging;

namespace DogSab.Platform.Core.Logging.Impl.Providers;

/// <summary>
/// A Microsoft.Extensions.Logging <see cref="ILoggerProvider"/> that creates
/// <see cref="RollingFileLogger"/> instances, all sharing a single
/// <see cref="RollingFileWriter"/> so file rotation is coordinated across
/// every category writing to the platform's log file.
/// </summary>
public sealed class RollingFileLoggerProvider : ILoggerProvider
{
    /// <summary>The shared writer used by every logger this provider creates.</summary>
    private readonly RollingFileWriter _writer;

    /// <summary>The minimum level passed to every created logger.</summary>
    private readonly LogLevel _minimumLevel;

    /// <summary>Loggers already created, keyed by category name, to avoid recreating one per request.</summary>
    private readonly ConcurrentDictionary<string, RollingFileLogger> _loggers = new();

    /// <summary>
    /// Creates a new rolling file logger provider.
    /// </summary>
    /// <param name="options">The logging options controlling file path, size limit, and retention.</param>
    public RollingFileLoggerProvider(LoggingOptions options)
    {
        var filePath = LogFilePathResolver.GetCurrentLogFilePath();
        _writer = new RollingFileWriter(filePath, options.MaxFileSizeBytes, options.RetainedFileCount);
        _minimumLevel = LogLevelMapper.ToMicrosoft(options.MinimumLevel);
    }

    /// <summary>
    /// Returns the logger for a given category, creating and caching a new one
    /// on first request.
    /// </summary>
    /// <param name="categoryName">The category name to create a logger for.</param>
    /// <returns>The logger for that category, backed by the shared rolling file writer.</returns>
    public ILogger CreateLogger(string categoryName)
    {
        return _loggers.GetOrAdd(categoryName, name => new RollingFileLogger(name, _writer, _minimumLevel));
    }
    
    /// <summary>
    /// Releases the shared file writer's resources. Individual
    /// <see cref="RollingFileLogger"/> instances hold no resources of their own.
    /// </summary>
    public void Dispose()
    {
        _writer.Dispose();
    }
}