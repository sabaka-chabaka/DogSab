using DogSab.Platform.Core.Abstractions.Lifecycle;
using DogSab.Platform.Core.Abstractions.Logging;
using DogSab.Platform.Core.Threading.Impl.Diagnostics;
using DogSab.Platform.Core.Threading.Impl.ReadWrite;

namespace DogSab.Platform.Core.Threading.Impl.Startup;

/// <summary>
/// Platform startup activity that optionally starts the <see cref="DeadlockDetector"/>
/// watchdog once the read/write lock has been constructed. Registered as an
/// <see cref="IStartupActivity"/> so it runs automatically during application
/// startup without any subsystem needing to remember to wire it up manually.
/// Runs very early (low <see cref="Order"/>) so the watchdog is active before
/// any other startup activity might perform a long-running write action.
/// </summary>
public sealed class ThreadingStartupActivity : IStartupActivity
{
     /// <summary>The read/write manager whose internal lock should be monitored, if diagnostics are enabled.</summary>
    private readonly ReadWriteActionManagerImpl _readWriteActionManager;

    /// <summary>Factory used to obtain loggers for the components started by this activity.</summary>
    private readonly ILoggerFactory _loggerFactory;

    /// <summary>Whether the deadlock watchdog should be started. Should be <c>false</c> in release/production builds.</summary>
    private readonly bool _enableDeadlockDetection;

    /// <summary>
    /// Creates a new threading startup activity.
    /// </summary>
    /// <param name="readWriteActionManager">The manager whose lock should be monitored.</param>
    /// <param name="loggerFactory">Factory used to obtain loggers for started diagnostics components.</param>
    /// <param name="enableDeadlockDetection">Whether to start the deadlock watchdog. Defaults to <c>false</c>.</param>
    public ThreadingStartupActivity(
        ReadWriteActionManagerImpl readWriteActionManager,
        ILoggerFactory loggerFactory,
        bool enableDeadlockDetection = false)
    {
        _readWriteActionManager = readWriteActionManager;
        _loggerFactory = loggerFactory;
        _enableDeadlockDetection = enableDeadlockDetection;
    }

    /// <summary>Runs before most other startup activities, so diagnostics are active as early as possible.</summary>
    public int Order => -1000;

    /// <summary>
    /// Starts the deadlock watchdog if enabled. Threading infrastructure itself
    /// (the lock, the dispatcher, the background queue) is already constructed by
    /// this point via the DI container — this activity only wires up optional diagnostics.
    /// </summary>
    /// <param name="cancellationToken">Token signaled if startup is aborted.</param>
    /// <returns>A completed task, since starting the watchdog is a synchronous, non-blocking operation.</returns>
    public Task RunActivityAsync(CancellationToken cancellationToken)
    {
        if (!_enableDeadlockDetection)
        {
            return Task.CompletedTask;
        }

        var logger = _loggerFactory.GetLogger(typeof(ThreadingStartupActivity));
        logger.Info("Starting deadlock watchdog for the platform read/write lock...");
        
        var detector = new DeadlockDetector(_readWriteActionManager.UnderlyingLock, _loggerFactory);
        detector.Start();
        
        return Task.CompletedTask;
    }
}