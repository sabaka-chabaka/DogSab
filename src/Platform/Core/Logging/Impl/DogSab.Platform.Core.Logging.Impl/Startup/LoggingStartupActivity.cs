using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using DogSab.Platform.Core.Abstractions.Lifecycle;
using DogSab.Platform.Core.Logging.Impl.Configuration;
using DogSabILoggerFactory = DogSab.Platform.Core.Abstractions.Logging.ILoggerFactory;

namespace DogSab.Platform.Core.Logging.Impl.Startup;

/// <summary>
/// Platform startup activity that writes an initial banner to the log once
/// logging is available: application version, OS/runtime info, and the resolved
/// log file path. Useful when troubleshooting a user's issue from a shared log
/// file, since this banner establishes exactly which build and environment
/// produced the rest of the log.
/// </summary>
public sealed class LoggingStartupActivity : IStartupActivity
{
    /// <summary>Factory used to obtain the logger this activity writes the banner through.</summary>
    private readonly DogSabILoggerFactory _loggerFactory;

    /// <summary>
    /// Creates a new logging startup activity.
    /// </summary>
    /// <param name="loggerFactory">Factory used to obtain the logger for the startup banner.</param>
    public LoggingStartupActivity(DogSabILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }

    /// <summary>
    /// Runs before nearly all other startup activities, so the banner appears
    /// at the very top of each session's log output. Slightly after
    /// <c>ThreadingStartupActivity</c> (Order -1000), since logging itself must
    /// already be fully constructed by the time this runs — which it is, having
    /// been built via <see cref="Bootstrap.LoggingBootstrapper"/> before the
    /// startup activity pipeline begins at all.
    /// </summary>
    public int Order => -900;

    /// <summary>
    /// Writes the startup banner: assembly version, OS description, runtime
    /// version, and the resolved log file path.
    /// </summary>
    /// <param name="cancellationToken">Token signaled if startup is aborted.</param>
    /// <returns>A completed task, since this activity performs only synchronous, in-memory work.</returns>
    public Task RunActivityAsync(CancellationToken cancellationToken)
    {
        var logger = _loggerFactory.GetLogger(typeof(LoggingStartupActivity));

        var version = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "Unknown";
        var osDescription = RuntimeInformation.OSDescription;
        var frameworkDescription = RuntimeInformation.FrameworkDescription;
        var logFilePath = LogFilePathResolver.GetCurrentLogFilePath();
        
        logger.Info("========================================");
        logger.Info("DogSab starting — version {0}", version);
        logger.Info("OS: {0}", osDescription);
        logger.Info("Runtime: {0}", frameworkDescription);
        logger.Info("Log file: {0}", logFilePath);
        logger.Info("========================================");
        
        return Task.CompletedTask;
    }
}