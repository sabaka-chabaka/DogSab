using DogSab.Platform.Core.Abstractions.Messaging;
using DogSab.Platform.Core.Messaging.Impl.Topics;

namespace DogSab.Platform.ProjectModel.Abstractions.Events;

/// <summary>
/// Declares the platform's project model change topic. A single topic
/// (unlike Vfs's split into UI/background variants) is sufficient here,
/// since structural project model changes are comparatively rare — modules
/// are added/removed occasionally, not on every keystroke — so forcing a
/// single <see cref="DeliveryMode.UiThread"/> delivery mode does not carry
/// the same performance risk that per-keystroke file change events would.
/// Subscribers that need to react on a background thread instead can simply
/// dispatch their own work off the UI thread inside their handler.
/// </summary>
public static class ProjectModelTopics
{
    /// <summary>Published whenever the project structure changes: a project or module is added or about to be removed.</summary>
    public static readonly ITopic<IProjectModelListener> MODEL_CHANGED =
        TopicImpl<IProjectModelListener>.Create("projectModel.changed", DeliveryMode.UiThread);
}