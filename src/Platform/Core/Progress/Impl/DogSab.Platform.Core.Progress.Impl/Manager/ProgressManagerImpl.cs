using DogSab.Platform.Core.Abstractions.Progress;
using DogSab.Platform.Core.Progress.Impl.Indicators;
using DogSab.Platform.Core.Progress.Impl.Tracking;

namespace DogSab.Platform.Core.Progress.Impl.Manager;

/// <summary>
/// Default implementation of <see cref="IProgressManager"/>.
/// Creates a fresh <see cref="ProgressIndicatorImpl"/> for each operation,
/// registers it as the calling thread's active indicator via
/// <see cref="ActiveProgressTracker"/> for the duration of the operation, and
/// clears it afterward regardless of whether the operation succeeded, failed,
/// or was canceled.
/// </summary>
public sealed class ProgressManagerImpl : IProgressManager
{
    /// <summary>Tracks the active indicator per thread, so nested code can retrieve it via <see cref="GetCurrentProgress"/>.</summary>
    private readonly ActiveProgressTracker _tracker = new();

    /// <summary>
    /// Runs an action synchronously while displaying a progress indicator.
    /// </summary>
    /// <param name="title">Title shown to the user describing the operation.</param>
    /// <param name="action">The action to run, receiving the progress indicator to report into.</param>
    /// <param name="canCancel">Whether the user is allowed to cancel the operation.</param>
    public void RunWithProgress(string title, Action<IProgressIndicator> action, bool canCancel = true)
    {
        var indicator = CreateIndicator(title, canCancel);
        
        _tracker.SetCurrent(indicator);
        try
        {
            action(indicator);
        }
        finally
        {
            _tracker.Clear();
        }
    }
    
    /// <summary>
    /// Runs an action asynchronously while displaying a progress indicator.
    /// </summary>
    /// <param name="title">Title shown to the user describing the operation.</param>
    /// <param name="action">The asynchronous action to run, receiving the progress indicator to report into.</param>
    /// <param name="canCancel">Whether the user is allowed to cancel the operation.</param>
    /// <returns>A task that completes when the operation finishes.</returns>
    public async Task RunWithProgressAsync(string title, Func<IProgressIndicator, Task> action, bool canCancel = true)
    {
        var indicator = CreateIndicator(title, canCancel);

        _tracker.SetCurrent(indicator);
        try
        {
            await action(indicator).ConfigureAwait(false);
        }
        finally
        {
            _tracker.Clear();
        }
    }

    /// <summary>
    /// Returns the progress indicator for the operation currently running on the
    /// calling thread, if any.
    /// </summary>
    /// <returns>The current <see cref="IProgressIndicator"/>, or <c>null</c> if none is active on this thread.</returns>
    public IProgressIndicator? GetCurrentProgress()
    {
        return _tracker.GetCurrent();
    }

    /// <summary>
    /// Creates and initializes a new indicator for an operation.
    /// </summary>
    /// <param name="title">The initial text to display, describing the operation.</param>
    /// <param name="canCancel">
    /// Whether the operation is cancellable. Currently informational only — this
    /// implementation does not prevent <see cref="IProgressIndicator.Cancel"/> from
    /// being called regardless; UI layers are expected to hide/disable a cancel
    /// button when <paramref name="canCancel"/> is <c>false</c> rather than relying
    /// on the indicator itself to reject cancellation.
    /// </param>
    /// <returns>A newly created, ready-to-use progress indicator.</returns>
    private static ProgressIndicatorImpl CreateIndicator(string title, bool canCancel)
    {
        _ = canCancel; // see remarks above; kept as a parameter for API symmetry and future enforcement

        return new ProgressIndicatorImpl
        {
            Text = title,
            IsIndeterminate = true
        };
    }
}