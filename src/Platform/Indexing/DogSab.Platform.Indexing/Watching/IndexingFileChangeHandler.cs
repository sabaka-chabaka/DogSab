using DogSab.Platform.Core.Abstractions.Messaging;
using DogSab.Platform.Indexing.Building;
using DogSab.Platform.Indexing.Index;
using DogSab.Platform.Vfs.Abstractions.Watching;

namespace DogSab.Platform.Indexing.Watching;

/// <summary>
/// Subscribes to <see cref="VfsTopics.FILE_CHANGED_BACKGROUND"/>: enqueues
/// changed files for reindexing, and for deleted files, removes their stale
/// entries from every declared index directly (there is nothing left to read
/// and re-derive entries from, so this bypasses <see cref="IndexBuildScheduler"/>
/// entirely for deletions).
/// </summary>
public sealed class IndexingFileChangeHandler : IFileChangeListener
{
    private readonly IndexBuildScheduler _scheduler;
    private readonly IndexRegistry _registry;

    public IndexingFileChangeHandler(IndexBuildScheduler scheduler, IndexRegistry registry, IMessageBus messageBus)
    {
        _scheduler = scheduler;
        _registry = registry;

        var connection = messageBus.Connect();
        connection.Subscribe(VfsTopics.FILE_CHANGED_BACKGROUND, this);
    }

    /// <inheritdoc />
    public void OnFileChanged(FileChangeEvent args)
    {
        if (args.File is not null)
        {
            _scheduler.EnqueueFile(args.File);
            return;
        }

        foreach (var indexId in _registry.AllIndexIds)
        {
            _registry.GetStorage(indexId).RemoveEntriesForFile(args.Path);
        }
    }
}