namespace DogSab.Platform.Core.Threading.Impl.Background;

/// <summary>
/// Internal wrapper pairing a unit of background work with the means to signal
/// its completion back to the original caller of <see cref="IBackgroundTaskQueue.Enqueue"/>,
/// since the work itself runs on a separate consumer loop rather than the caller's task.
/// </summary>
public sealed class BackgroundTaskQueueItem
{
    /// <summary>The work to execute, receiving a cancellation token tied to queue shutdown.</summary>
    public Func<CancellationToken, Task> Work { get; }

    /// <summary>Signaled with the work's outcome once it has run, so the original caller's task completes.</summary>
    public TaskCompletionSource CompletionSource { get; }

    /// <summary>
    /// Creates a new queue item.
    /// </summary>
    /// <param name="work">The asynchronous work to execute.</param>
    public BackgroundTaskQueueItem(Func<CancellationToken, Task> work)
    {
        Work = work;
        CompletionSource = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    }
}