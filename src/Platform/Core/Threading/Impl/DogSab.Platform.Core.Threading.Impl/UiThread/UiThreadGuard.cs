using System.Diagnostics;
using DogSab.Platform.Core.Abstractions.Logging;

namespace DogSab.Platform.Core.Threading.Impl.UiThread;

/// <summary>
/// Verifies that calling code is running on the UI thread, throwing a descriptive
/// exception with a captured stack trace when it is not. Kept separate from
/// <see cref="AvaloniaUiThreadDispatcher"/> so the check-and-report logic can be
/// unit tested independently of any real Avalonia dispatcher instance.
/// </summary>
public sealed class UiThreadGuard
{
    /// <summary>Logger used to report violations before the exception propagates.</summary>
    private readonly ILogger _logger;
    
    /// <summary>Delegate that reports whether the calling thread is currently the UI thread.</summary>
    private readonly Func<bool> _isUiThreadCheck;

    /// <summary>
    /// Creates a new guard.
    /// </summary>
    /// <param name="loggerFactory">Factory used to obtain a logger scoped to this guard.</param>
    /// <param name="isUiThreadCheck">Delegate returning whether the calling thread is the UI thread.</param>
    public UiThreadGuard(ILoggerFactory loggerFactory, Func<bool> isUiThreadCheck)
    {
        _logger = loggerFactory.GetLogger(typeof(UiThreadGuard));
        _isUiThreadCheck = isUiThreadCheck;
    }
    
    /// <summary>
    /// Throws an <see cref="InvalidOperationException"/> if the calling thread is not
    /// the UI thread. Logs the violating call's stack trace before throwing, since the
    /// exception may otherwise surface far from the actual offending call site
    /// (e.g. swallowed by a background task's fire-and-forget continuation).
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when called off the UI thread.</exception>
    public void Verify()
    {
        if (_isUiThreadCheck())
        {
            return;
        }

        var stackTrace = new StackTrace(skipFrames: 1, fNeedFileInfo: true);

        _logger.Error(
            "UI thread violation detected on managed thread {0}. Call stack:\n{1}",
            exception: null,
            Environment.CurrentManagedThreadId,
            stackTrace);

        throw new InvalidOperationException(
            $"This operation must be called from the UI thread, but was called from thread " +
            $"{Environment.CurrentManagedThreadId} ('{System.Threading.Thread.CurrentThread.Name ?? "unnamed"}').");
    }
}