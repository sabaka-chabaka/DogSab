namespace DogSab.Platform.Core.Abstractions.Threading;

/// <summary>A queue for scheduling work on background threads without blocking the UI.</summary>
public interface IBackgroundTaskQueue
{
    /// <summary>
    /// Schedules a unit of work to run on a background thread.
    /// </summary>
    /// <param name="work">The asynchronous work to execute, receiving a cancellation token.</param>
    /// <param name="priority">The relative priority used to order queued work.</param>
    /// <returns>A task that completes when the work has finished.</returns>
    Task Enqueue(Func<CancellationToken, Task> work, BackgroundTaskPriority priority = BackgroundTaskPriority.Normal);
}

/// <summary>Relative priority of a queued background task.</summary>
public enum BackgroundTaskPriority
{
    /// <summary>Low-priority work, executed after all normal and high-priority work.</summary>
    Low,

    /// <summary>Default priority for most background work.</summary>
    Normal,

    /// <summary>High-priority work, executed ahead of normal and low-priority work.</summary>
    High
}