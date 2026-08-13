using DogSab.Platform.Core.Abstractions.Messaging;
using DogSab.Platform.Core.Messaging.Impl.Topics;

namespace DogSab.Platform.Indexing.Abstractions.Events;

/// <summary>
/// Declares the platform's indexing lifecycle topic. Delivered on the UI
/// thread, since its primary subscriber is UI (a status bar progress
/// indicator) and indexing progress notifications are naturally throttled by
/// <see cref="Building.IndexBuildScheduler"/> (not published per-file at high
/// frequency), unlike file change events — so, unlike the earlier concern
/// raised about <c>ProjectModelTopics.MODEL_CHANGED</c>, forcing UI-thread
/// delivery here does not risk blocking the UI on heavy synchronous work,
/// since the listener side only ever does cheap UI updates, never the
/// indexing work itself.
/// </summary>
public static class IndexingTopics
{
    /// <summary>Published on indexing start, progress, and completion.</summary>
    public static readonly ITopic<IIndexingListener> INDEXING_STATE_CHANGED =
        TopicImpl<IIndexingListener>.Create("indexing.stateChanged", DeliveryMode.UiThread);
}