using System.Threading.Channels;
using DogSab.Platform.Core.Abstractions.Logging;
using DogSab.Platform.Core.Abstractions.Threading;

namespace DogSab.Platform.Core.Threading.Impl.Background;


/// <summary>
/// Runs queued background work on a dedicated consumer loop, always preferring
/// higher-priority items: all currently available <see cref="BackgroundTaskPriority.High"/>
/// items are drained before any <see cref="BackgroundTaskPriority.Normal"/> item runs,
/// and likewise Normal before <see cref="BackgroundTaskPriority.Low"/>.
/// Items within the same priority run in FIFO order. Work items run sequentially,
/// one at a time, on a single background thread — this keeps ordering predictable
/// and avoids oversubscribing the thread pool with platform-internal work.
/// </summary>
public sealed class PriorityTaskScheduler : IDisposable
{
    /// <summary>Channel holding queued high-priority items.</summary>
    private readonly Channel<BackgroundTaskQueueItem> _highPriorityChannel = Channel.CreateUnbounded<BackgroundTaskQueueItem>();

    /// <summary>Channel holding queued normal-priority items.</summary>
    private readonly Channel<BackgroundTaskQueueItem> _normalPriorityChannel = Channel.CreateUnbounded<BackgroundTaskQueueItem>();

    /// <summary>Channel holding queued low-priority items.</summary>
    private readonly Channel<BackgroundTaskQueueItem> _lowPriorityChannel = Channel.CreateUnbounded<BackgroundTaskQueueItem>();

    /// <summary>Signaled whenever an item is enqueued into any channel, to wake the consumer loop.</summary>
    private readonly SemaphoreSlim _itemAvailableSignal = new(0);

    /// <summary>Canceled on <see cref="Dispose"/> to stop the consumer loop.</summary>
    private readonly CancellationTokenSource _shutdownTokenSource = new();

    /// <summary>Logger used to report unhandled exceptions from queued work.</summary>
    private readonly ILogger _logger;

    /// <summary>The task running the consumer loop, started in the constructor.</summary>
    private readonly Task _consumerLoopTask;

    /// <summary>
    /// Creates a new priority task scheduler and immediately starts its consumer loop
    /// on a dedicated background thread.
    /// </summary>
    /// <param name="loggerFactory">Factory used to get a logger scoped to this scheduler.</param>
    public PriorityTaskScheduler(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.GetLogger(typeof(PriorityTaskScheduler));
        _consumerLoopTask = Task.Factory.StartNew(
            RunConsumerLoopAsync,
            CancellationToken.None,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default).Unwrap();
    }

    /// <summary>
    /// Enqueues an item for execution at the given priority. Wakes the consumer
    /// loop if it is currently idle.
    /// </summary>
    /// <param name="item">The item to enqueue.</param>
    /// <param name="priority">The priority determining how soon it runs relative to other queued items.</param>
    public void Enqueue(BackgroundTaskQueueItem item, BackgroundTaskPriority priority)
    {
        var channel = SelectChannel(priority);
        channel.Writer.TryWrite(item);
        _itemAvailableSignal.Release();
    }

    /// <summary>
    /// Returns the channel writer/reader pair associated with a given priority.
    /// </summary>
    /// <param name="priority">The priority to look up.</param>
    /// <returns>The channel used to queue items of that priority.</returns>
    private Channel<BackgroundTaskQueueItem> SelectChannel(BackgroundTaskPriority priority)
    {
        return priority switch
        {
            BackgroundTaskPriority.High => _highPriorityChannel,
            BackgroundTaskPriority.Normal => _normalPriorityChannel,
            BackgroundTaskPriority.Low => _lowPriorityChannel,
            _ => _normalPriorityChannel
        };
    }

    /// <summary>
    /// The main consumer loop: waits for an item to become available, then drains
    /// and executes items strictly in priority order (High, then Normal, then Low)
    /// until all channels are empty, then waits again. Runs until shutdown is requested.
    /// </summary>
    private async Task RunConsumerLoopAsync()
    {
        var shutdownToken = _shutdownTokenSource.Token;

        while (!shutdownToken.IsCancellationRequested)
        {
            try
            {
                await _itemAvailableSignal.WaitAsync(shutdownToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            while (TryDequeueNextByPriority(out var item))
            {
                await ExecuteAsync(item!, shutdownToken).ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// Attempts to dequeue the next item respecting priority order: high, then normal, then low.
    /// </summary>
    /// <param name="item">The dequeued item, if one was available.</param>
    /// <returns><c>true</c> if an item was dequeued; otherwise <c>false</c>.</returns>
    private bool TryDequeueNextByPriority(out BackgroundTaskQueueItem? item)
    {
        if (_highPriorityChannel.Reader.TryRead(out item))
        {
            return true;
        }

        if (_normalPriorityChannel.Reader.TryRead(out item))
        {
            return true;
        }

        if (_lowPriorityChannel.Reader.TryRead(out item))
        {
            return true;
        }

        item = null;
        return false;
    }

    /// <summary>
    /// Executes a single queued item, propagating its outcome to the item's
    /// <see cref="BackgroundTaskQueueItem.CompletionSource"/>. Exceptions are caught,
    /// logged, and reported through the completion source rather than crashing the
    /// consumer loop, so a single failing task does not stop the queue.
    /// </summary>
    /// <param name="item">The item to execute.</param>
    /// <param name="shutdownToken">Token passed to the work, signaled on scheduler shutdown.</param>
    private async Task ExecuteAsync(BackgroundTaskQueueItem item, CancellationToken shutdownToken)
    {
        try
        {
            await item.Work(shutdownToken).ConfigureAwait(false);
            item.CompletionSource.TrySetResult();
        }
        catch (OperationCanceledException) when (shutdownToken.IsCancellationRequested)
        {
            item.CompletionSource.TrySetCanceled(shutdownToken);
        }
        catch (Exception ex)
        {
            _logger.Error("Unhandled exception in background task", ex);
            item.CompletionSource.TrySetException(ex);
        }
    }

    /// <summary>
    /// Signals the consumer loop to stop after finishing any item currently executing
    /// and waits briefly for it to exit. Items still queued at shutdown are left
    /// unexecuted, and their completion sources are never completed.
    /// </summary>
    public void Dispose()
    {
        _shutdownTokenSource.Cancel();
        _itemAvailableSignal.Release(); // wake the loop so it observes cancellation promptly

        try
        {
            _consumerLoopTask.Wait(TimeSpan.FromSeconds(5));
        }
        catch (AggregateException)
        {
            // expected if the loop observed cancellation via the awaited semaphore
        }

        _shutdownTokenSource.Dispose();
        _itemAvailableSignal.Dispose();
    }
}