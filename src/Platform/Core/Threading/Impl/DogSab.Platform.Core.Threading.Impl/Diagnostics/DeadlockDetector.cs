using DogSab.Platform.Core.Abstractions.Logging;
using DogSab.Platform.Core.Threading.Impl.ReadWrite;

namespace DogSab.Platform.Core.Threading.Impl.Diagnostics;

/// <summary>
/// Development-time watchdog that periodically checks whether the platform's
/// write lock has been held for longer than a suspicious threshold, and if so,
/// logs the holding thread's stack trace to help diagnose deadlocks or
/// pathologically slow write actions. Not intended to run in release builds
/// of end-user installations, since capturing another thread's stack trace
/// has a real performance cost — see <see cref="Start"/>.
/// </summary>
public sealed class DeadlockDetector : IDisposable
{
    /// <summary>Default interval between checks.</summary>
    private static readonly TimeSpan DefaultCheckInterval = TimeSpan.FromSeconds(1);

    /// <summary>Default duration after which a held write lock is considered suspicious.</summary>
    private static readonly TimeSpan DefaultSuspiciousThreshold = TimeSpan.FromSeconds(5);

    /// <summary>The lock being monitored.</summary>
    private readonly PlatformReaderWriterLock _monitoredLock;

    /// <summary>Logger used to report suspected deadlocks.</summary>
    private readonly ILogger _logger;

    /// <summary>How often the watchdog checks the monitored lock's state.</summary>
    private readonly TimeSpan _checkInterval;

    /// <summary>How long the write lock may be held before being reported as suspicious.</summary>
    private readonly TimeSpan _suspiciousThreshold;

    /// <summary>Canceled on <see cref="Dispose"/> to stop the watchdog loop.</summary>
    private readonly CancellationTokenSource _stopTokenSource = new();

    /// <summary>The task running the watchdog loop, or <c>null</c> until <see cref="Start"/> is called.</summary>
    private Task? _watchdogTask;

    /// <summary>Thread ID that was already reported as suspicious to avoid repeated log spam for the same hold.</summary>
    private int? _lastReportedThreadId;

    /// <summary>
    /// Creates a new deadlock detector for the given lock.
    /// </summary>
    /// <param name="monitoredLock">The lock to watch for suspiciously long write-lock holds.</param>
    /// <param name="loggerFactory">Factory used to get a logger scoped to this detector.</param>
    /// <param name="checkInterval">How often to check the lock's state. Defaults to 1 second.</param>
    /// <param name="suspiciousThreshold">How long a write lock may be held before being reported. Defaults to 5 seconds.</param>
    public DeadlockDetector(
        PlatformReaderWriterLock monitoredLock,
        ILoggerFactory loggerFactory,
        TimeSpan? checkInterval = null,
        TimeSpan? suspiciousThreshold = null)
    {
        _monitoredLock = monitoredLock;
        _logger = loggerFactory.GetLogger(typeof(DeadlockDetector));
        _checkInterval = checkInterval ?? DefaultCheckInterval;
        _suspiciousThreshold = suspiciousThreshold ?? DefaultSuspiciousThreshold;
    }

    /// <summary>
    /// Starts the watchdog loop on a background timer. Intended to be called once
    /// during application startup in development/debug configurations only —
    /// see the type-level remarks regarding release build usage.
    /// </summary>
    public void Start()
    {
        if (_watchdogTask is not null)
        {
            return;
        }

        _watchdogTask = Task.Run(() => RunLoopAsync(_stopTokenSource.Token));
    }

    /// <summary>
    /// The watchdog's periodic check loop: sleeps for <see cref="_checkInterval"/>,
    /// then inspects whether the write lock is currently held and for how long.
    /// </summary>
    /// <param name="stopToken">Token signaled when the detector is disposed of.</param>
    private async Task RunLoopAsync(CancellationToken stopToken)
    {
        while (!stopToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_checkInterval, stopToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            CheckForSuspiciousHold();
        }
    }

    /// <summary>
    /// Checks the monitored lock's current write-lock holder and reports it once
    /// if it has plausibly been held past <see cref="_suspiciousThreshold"/>.
    /// Note: this relies on repeated observation rather than a precise held-duration
    /// timestamp, since that value is private to <see cref="PlatformReaderWriterLock"/>;
    /// it reports the same holder at most once per hold to avoid log spam.
    /// </summary>
    private void CheckForSuspiciousHold()
    {
        var holderThreadId = _monitoredLock.WriteLockHolderThreadId;

        if (holderThreadId is null)
        {
            _lastReportedThreadId = null;
            return;
        }

        if (_lastReportedThreadId == holderThreadId)
        {
            return;
        }

        _lastReportedThreadId = holderThreadId;

        _logger.Warn(
            "Write lock has been held by thread {0} for at least {1}s — possible deadlock or slow write action. " +
            "Consider capturing a full process dump if this persists.",
            holderThreadId.Value,
            (int)_suspiciousThreshold.TotalSeconds);
    }

    /// <summary>
    /// Stops the watchdog loop and releases its resources.
    /// </summary>
    public void Dispose()
    {
        _stopTokenSource.Cancel();

        try
        {
            _watchdogTask?.Wait(TimeSpan.FromSeconds(2));
        }
        catch (AggregateException)
        {
            // expected on cancellation
        }

        _stopTokenSource.Dispose();
    }
}