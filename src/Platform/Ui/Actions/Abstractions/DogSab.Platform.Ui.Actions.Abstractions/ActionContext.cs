using DogSab.Platform.ProjectModel.Abstractions.Project;

namespace DogSab.Platform.Ui.Actions.Abstractions;

/// <summary>
/// The contextual information available to an <see cref="AnAction"/> when
/// it's asked whether it's enabled and when it executes — e.g. the currently
/// active project, so an action like "Build Project" can determine which
/// project to act on without needing that threaded through as an explicit
/// parameter by every caller (the same "ambient context" pattern used by
/// <c>ICurrentProjectAccessor</c>, but scoped specifically to a single
/// action invocation rather than an arbitrary async flow).
/// </summary>
public readonly struct ActionContext
{
    /// <summary>The project currently active in the UI, or <c>null</c> if none is open.</summary>
    public ProjectId? ActiveProjectId { get; }

    /// <summary>
    /// Arbitrary additional context data keyed by string, for action-specific
    /// needs the platform doesn't have a first-class property for (e.g. the
    /// currently selected file in Project View). Kept as an escape hatch
    /// rather than growing this struct's fixed fields indefinitely for every
    /// possible action's needs.
    /// </summary>
    private readonly System.Collections.Generic.IReadOnlyDictionary<string, object?> _data;

    /// <summary>
    /// Creates a new action context.
    /// </summary>
    /// <param name="activeProjectId">The currently active project, or <c>null</c>.</param>
    /// <param name="data">Additional context data.</param>
    public ActionContext(ProjectId? activeProjectId, System.Collections.Generic.IReadOnlyDictionary<string, object?>? data = null)
    {
        ActiveProjectId = activeProjectId;
        _data = data ?? new System.Collections.Generic.Dictionary<string, object?>();
    }

    /// <summary>
    /// Retrieves a piece of additional context data by key, typed.
    /// </summary>
    /// <typeparam name="T">The expected type of the value.</typeparam>
    /// <param name="key">The key to look up.</param>
    /// <returns>The value, or <c>default</c> if the key is absent or the stored value isn't of type <typeparamref name="T"/>.</returns>
    public T? GetData<T>(string key)
    {
        return _data.TryGetValue(key, out var value) && value is T typed ? typed : default;
    }
}