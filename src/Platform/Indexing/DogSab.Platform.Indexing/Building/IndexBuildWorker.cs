using DogSab.Platform.Core.Abstractions.Logging;
using DogSab.Platform.Indexing.Abstractions.Index;
using DogSab.Platform.Indexing.Index;
using DogSab.Platform.Vfs.Abstractions.VirtualFile;

namespace DogSab.Platform.Indexing.Building;

/// <summary>
/// Runs every registered index extension against a single file: removes the
/// file's stale entries from each extension's storage, then re-derives and
/// re-adds current entries via <see cref="IIndexExtension.IndexUntyped"/>.
/// Called once per file per (re)index request, from
/// <see cref="IndexBuildScheduler"/>'s background queue — never on the UI thread.
/// </summary>
public sealed class IndexBuildWorker
{
    private readonly IndexRegistry _registry;
    private readonly ILogger _logger;

    public IndexBuildWorker(IndexRegistry registry, ILoggerFactory loggerFactory)
    {
        _registry = registry;
        _logger = loggerFactory.GetLogger(typeof(IndexBuildWorker));
    }

    /// <summary>
    /// (Re)indexes a single file against every declared index whose
    /// extension applies to it.
    /// </summary>
    /// <param name="file">The file to index.</param>
    /// <param name="indexIds">Every currently declared index ID.</param>
    public void IndexFile(IVirtualFile file, System.Collections.Generic.IReadOnlyCollection<IndexId> indexIds)
    {
        foreach (var indexId in indexIds)
        {
            try
            {
                IndexFileForIndex(file, indexId);
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to index file '{0}' for index '{1}'", ex, file.Path, indexId);
            }
        }
    }

    /// <summary>
    /// Runs a single index's extension against a file, entirely through the
    /// untyped path — checks applicability, clears the file's stale entries,
    /// and writes fresh ones, all without knowing the index's specific (TKey, TValue).
    /// </summary>
    private void IndexFileForIndex(IVirtualFile file, IndexId indexId)
    {
        var extension = _registry.GetExtensionUntyped(indexId);

        if (!extension.AppliesTo(file))
        {
            return;
        }

        var storage = _registry.GetStorage(indexId);
        storage.RemoveEntriesForFile(file.Path);

        foreach (var pair in extension.IndexUntyped(file))
        {
            storage.AddEntry(pair.Key, pair.Value, file.Path);
        }
    }
}