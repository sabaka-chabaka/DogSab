namespace DogSab.Platform.Core.Abstractions.Progress;

/// <summary>Runs long-running operations with a visible, optionally cancellable progress indicator.</summary>
public interface IProgressManager
{
    /// <summary>
    /// Runs an action synchronously while displaying a progress indicator.
    /// </summary>
    /// <param name="title">Title shown to the user describing the operation.</param>
    /// <param name="action">The action to run, receiving the progress indicator to report into.</param>
    /// <param name="canCancel">Whether the user is allowed to cancel the operation.</param>
    void RunWithProgress(string title, Action<IProgressIndicator> action, bool canCancel = true);

    /// <summary>
    /// Runs an action asynchronously while displaying a progress indicator.
    /// </summary>
    /// <param name="title">Title shown to the user describing the operation.</param>
    /// <param name="action">The asynchronous action to run, receiving the progress indicator to report into.</param>
    /// <param name="canCancel">Whether the user is allowed to cancel the operation.</param>
    /// <returns>A task that completes when the operation finishes.</returns>
    Task RunWithProgressAsync(string title, Func<IProgressIndicator, Task> action, bool canCancel = true);

    /// <summary>
    /// Returns the progress indicator for the operation currently running on the calling thread, if any.
    /// </summary>
    /// <returns>The current <see cref="IProgressIndicator"/>, or <c>null</c> if none is active.</returns>
    IProgressIndicator? GetCurrentProgress();
}