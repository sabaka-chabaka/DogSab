namespace DogSab.Platform.Core.Abstractions.Project;

/// <summary>
/// Provides access to the identifier of the project associated with the code
/// currently executing, without requiring that identifier to be threaded
/// through every intermediate method call as an explicit parameter. Backed by
/// an ambient, async-flowing context (not a thread-local one), since
/// platform code frequently continues execution on a different thread after
/// an <c>await</c>, and the active project must still be correctly observed
/// after such a continuation.
/// </summary>
public interface ICurrentProjectAccessor
{
    /// <summary>
    /// The identifier of the project associated with the currently executing
    /// code, or <c>null</c> if no project scope is currently active (e.g. code
    /// running at the application level, before any project is open).
    /// </summary>
    Guid? CurrentProjectId { get; }

    /// <summary>
    /// Marks <paramref name="projectId"/> as the active project for the
    /// duration of the returned scope. Nested calls are supported: disposing
    /// the returned scope restores whatever project ID (if any) was active
    /// before this call.
    /// </summary>
    /// <param name="projectId">The project ID to make active for the scope's lifetime.</param>
    /// <returns>A disposable that restores the previous project ID when disposed.</returns>
    IDisposable EnterProjectScope(Guid projectId);
}