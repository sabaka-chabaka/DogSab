
using System.Collections.Concurrent;
using DogSab.Platform.Core.Abstractions.Logging;
using DogSab.Platform.Core.Abstractions.Messaging;
using DogSab.Platform.Core.Abstractions.Threading;
using DogSab.Platform.Indexing.Abstractions.Events;
using DogSab.Platform.Indexing.Dumb;
using DogSab.Platform.Indexing.Index;
using DogSab.Platform.Vfs.Abstractions.VirtualFile;

namespace DogSab.Platform.Indexing.Building;

/// <summary>
/// Queues files for (re)indexing onto the platform's background task queue,
/// coordinating with <see cref="DumbServiceImpl"/> to enter dumb mode while a
/// batch is in progress and return to smart mode once the queue drains, and
/// publishing progress on <see cref="IndexingTopics.INDEXING_STATE_CHANGED"/>.
/// Multiple files enqueued in quick succession (e.g. an initial project scan)
/// are processed as a single batch, so dumb mode is entered once and the UI
/// sees one continuous progress report rather than flickering in and out of
/// dumb mode between individual files.
/// </summary>
public sealed class IndexBuildScheduler
{
    private readonly IndexBuildWorker _worker;
    private readonly IndexRegistry _registry;
    private readonly DumbServiceImpl _dumbService;
    private readonly IBackgroundTaskQueue _backgroundTaskQueue;
    private readonly IMessageBus _messageBus;
    private readonly ILogger _logger;

    /// <summary>Files currently queued or being processed in the active batch, used to report progress totals.</summary>
    private readonly ConcurrentQueue<IVirtualFile> _pendingFiles = new();

    /// <summary>Guards the batch-lifecycle state (whether a batch is currently active) against concurrent enqueue calls.</summary>
    private readonly object _batchLock = new();

    private bool _batchActive;
    private int _batchTotalFiles;
    private int _batchProcessedFiles;

    public IndexBuildScheduler(
        IndexBuildWorker worker,
        IndexRegistry registry,
        DumbServiceImpl dumbService,
        IBackgroundTaskQueue backgroundTaskQueue,
        IMessageBus messageBus,
        ILoggerFactory loggerFactory)
    {
        _worker = worker;
        _registry = registry;
        _dumbService = dumbService;
        _backgroundTaskQueue = backgroundTaskQueue;
        _messageBus = messageBus;
        _logger = loggerFactory.GetLogger(typeof(IndexBuildScheduler));
    }

    /// <summary>
    /// Enqueues a single file for (re)indexing. Starts a new batch (entering
    /// dumb mode and publishing <see cref="IIndexingListener.IndexingStarted"/>)
    /// if none is currently active; otherwise the file joins the active batch.
    /// </summary>
    /// <param name="file">The file to (re)index.</param>
    public void EnqueueFile(IVirtualFile file)
    {
        _pendingFiles.Enqueue(file);

        lock (_batchLock)
        {
            _batchTotalFiles++;

            if (!_batchActive)
            {
                _batchActive = true;
                _dumbService.EnterDumbMode();
                _messageBus.Publisher(IndexingTopics.INDEXING_STATE_CHANGED).IndexingStarted();
                _ = _backgroundTaskQueue.Enqueue(RunBatchAsync, BackgroundTaskPriority.Low);
            }
        }
    }

    /// <summary>
    /// Enqueues every file under a set of content roots, typically called
    /// once at startup for the project's initial index build.
    /// </summary>
    /// <param name="files">The files to enqueue.</param>
    public void EnqueueFiles(IEnumerable<IVirtualFile> files)
    {
        foreach (var file in files)
        {
            EnqueueFile(file);
        }
    }

    /// <summary>
    /// Drains the pending file queue, indexing each file and publishing
    /// progress, until empty — at which point the batch ends, the platform
    /// returns to smart mode, and <see cref="IIndexingListener.IndexingFinished"/> is published.
    /// New files enqueued while draining extend the same batch rather than
    /// starting a second one, since <see cref="EnqueueFile"/> only starts a
    /// new background task when no batch is currently active.
    /// </summary>
    private async Task RunBatchAsync(CancellationToken cancellationToken)
    {
        var indexIds = _registry.AllIndexIds;

        try
        {
            while (_pendingFiles.TryDequeue(out var file))
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    _worker.IndexFile(file, indexIds);
                }
                catch (Exception ex)
                {
                    _logger.Error("Unhandled failure indexing file '{0}'", ex, file.Path);
                }

                var processed = Interlocked.Increment(ref _batchProcessedFiles);
                _messageBus.Publisher(IndexingTopics.INDEXING_STATE_CHANGED)
                    .IndexingProgress(processed, Volatile.Read(ref _batchTotalFiles));
            }
        }
        finally
        {
            lock (_batchLock)
            {
                // Re-check: a file enqueued between the last TryDequeue
                // returning false and this lock being taken must not be lost.
                if (_pendingFiles.IsEmpty)
                {
                    _batchActive = false;
                    _batchTotalFiles = 0;
                    _batchProcessedFiles = 0;

                    _dumbService.EnterSmartMode();
                    _messageBus.Publisher(IndexingTopics.INDEXING_STATE_CHANGED).IndexingFinished();
                }
                else
                {
                    _ = _backgroundTaskQueue.Enqueue(RunBatchAsync, BackgroundTaskPriority.Low);
                }
            }
        }
    }
}