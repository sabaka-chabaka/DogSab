using DogSab.Platform.Core.Abstractions.Messaging;
using DogSab.Platform.Core.Messaging.Impl.Topics;
using DogSab.Platform.Extensibility.Abstractions.ExtensionPoints;

namespace DogSab.Platform.Vcs.Abstractions.Events;

/// <summary>
/// Declares the platform's VCS provider extension point and status change
/// topic.
/// </summary>
public static class VcsExtensionPoints
{
    /// <summary>
    /// Contributes a version control system integration.
    /// Application-scoped — a VCS plugin registers its provider once for
    /// the whole process; which provider actually applies to a given
    /// project directory is resolved separately at query time via
    /// <see cref="IVcsProvider.OwnsDirectory"/>, not by project-scoping the
    /// registration itself.
    /// </summary>
    public static readonly ExtensionPointName<IVcsProvider> VCS_PROVIDER =
        ExtensionPointName<IVcsProvider>.Create(
            "vcs.provider",
            "Provides version control integration for a specific VCS.");

    /// <summary>
    /// Published whenever a file's VCS status changes. Delivered
    /// synchronously rather than on the UI thread, since a status change
    /// can be detected from a background file-watcher context (much like
    /// <c>Vfs.Abstractions.Watching.VfsTopics.FILE_CHANGED_BACKGROUND</c>),
    /// and subscribers that need to update UI are expected to marshal onto
    /// the UI thread themselves within their handler — the same
    /// responsibility-on-the-subscriber approach already adopted for
    /// <c>ProjectModelTopics.MODEL_CHANGED</c>'s heavy-handler concern,
    /// applied here proactively rather than after the fact.
    /// </summary>
    public static readonly ITopic<IVcsStatusListener> VCS_STATUS_CHANGED =
        TopicImpl<IVcsStatusListener>.Create("vcs.statusChanged", DeliveryMode.Synchronous);
}