namespace DogSab.Platform.Indexing.Abstractions.Events;

/// <summary>
/// Listener interface for indexing lifecycle notifications, published on
/// <see cref="IndexingTopics.INDEXING_STATE_CHANGED"/>. Subscribers (e.g. a
/// status bar widget showing "Indexing...") react to indexing starting,
/// progressing, and finishing, without polling <c>IDumbService</c> themselves.
/// </summary>
public interface IIndexingListener
{
    /// <summary>Called when a batch of indexing work begins (e.g. initial project scan, or a reindex triggered by file changes).</summary>
    void IndexingStarted();

    /// <summary>
    /// Called periodically while indexing is in progress, reporting how many
    /// files have been processed out of the currently known total.
    /// </summary>
    /// <param name="filesProcessed">The number of files processed so far in the current batch.</param>
    /// <param name="totalFiles">The total number of files known to be queued for the current batch.</param>
    void IndexingProgress(int filesProcessed, int totalFiles);

    /// <summary>Called when the current batch of indexing work completes and the platform returns to <c>DumbModeState.Smart</c>.</summary>
    void IndexingFinished();
}