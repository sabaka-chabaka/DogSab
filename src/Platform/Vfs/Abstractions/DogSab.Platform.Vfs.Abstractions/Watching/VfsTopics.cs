using DogSab.Platform.Core.Abstractions.Messaging;

namespace DogSab.Platform.Vfs.Abstractions.Watching;

/// <summary>
/// Declares the platform's two file-change topics, split by delivery mode
/// rather than a single topic, since subscribers have genuinely different
/// threading needs: UI components (e.g. the Project View tool window) must
/// react on the UI thread, while heavy background consumers (e.g. Indexing)
/// must NOT be forced onto the UI thread, since that would block the UI for
/// the duration of a potentially expensive re-index. A single topic can only
/// have one fixed <see cref="DeliveryMode"/>, so two topics — not one with a
/// per-subscriber choice — are declared here.
/// </summary>
public static class VfsTopics
{
    /// <summary>
    /// File change notifications delivered on the UI thread. Subscribe here
    /// for listeners that directly touch UI state (e.g. refreshing a tree view).
    /// </summary>
    public static readonly ITopic<IFileChangeListener> FILE_CHANGED_UI =
        Core.Messaging.Impl.Topics.TopicImpl<IFileChangeListener>.Create("vfs.fileChanged.ui", DeliveryMode.UiThread);

    /// <summary>
    /// File change notifications delivered synchronously on whichever thread
    /// detected the change. Subscribe here for listeners that perform
    /// background work (e.g. re-indexing) and must not be marshalled onto the UI thread.
    /// </summary>
    public static readonly ITopic<IFileChangeListener> FILE_CHANGED_BACKGROUND =
        Core.Messaging.Impl.Topics.TopicImpl<IFileChangeListener>.Create("vfs.fileChanged.background", DeliveryMode.Synchronous);
}