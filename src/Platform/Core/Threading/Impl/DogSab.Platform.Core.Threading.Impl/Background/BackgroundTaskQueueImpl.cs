using DogSab.Platform.Core.Abstractions.Logging;
using DogSab.Platform.Core.Abstractions.Threading;

namespace DogSab.Platform.Core.Threading.Impl.Background;

/// <summary>
/// Default implementation of <see cref="IBackgroundTaskQueue"/>.
/// Delegates actual execution to a single shared <see cref="PriorityTaskScheduler"/>,
/// so all platform-internal background work is scheduled through one predictable,
/// priority-ordered pipeline instead of raw <see cref="Task.Run(Action)"/> calls
/// scattered across the codebase.
/// </summary>
public sealed class BackgroundTaskQueueImpl : IBackgroundTaskQueue, IDisposable
{
    /// <summary>The scheduler that actually runs queued work.</summary>
    private readonly PriorityTaskScheduler _scheduler;
    
    /// <summary>
    /// Creates a new background task queue backed by its own priority scheduler.
    /// </summary>
    /// <param name="loggerFactory">Factory used to get a logger for the underlying scheduler.</param>
    public BackgroundTaskQueueImpl(ILoggerFactory loggerFactory)
    {
        _scheduler = new PriorityTaskScheduler(loggerFactory);
    }

    /// <summary>
    /// Schedules a unit of work to run on a background thread at the given priority.
    /// </summary>
    /// <param name="work">The asynchronous work to execute, receiving a cancellation token</param>
    /// <param name="priority">The relative priority used to order queued work.</param>
    /// <returns>A task that completes when the work has finished, faulted, or been canceled by shutdown.</returns>
    public Task Enqueue(Func<CancellationToken, Task> work, BackgroundTaskPriority priority = BackgroundTaskPriority.Normal)
    {
        var item = new BackgroundTaskQueueItem(work);
        _scheduler.Enqueue(item, priority);
        return item.CompletionSource.Task;
    }

    /// <summary>
    /// Shuts down the underlying scheduler, stopping the consumer loop after
    /// the currently executing item (if any) finishes.
    /// </summary>
    public void Dispose()
    {
        _scheduler.Dispose();
    }
}