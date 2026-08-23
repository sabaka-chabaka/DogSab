using DogSab.Platform.ProjectModel.Abstractions.Project;

namespace DogSab.Platform.Ui.Shell;

/// <summary>
/// Tracks which project is currently active in the UI. Distinct from
/// <c>Core.Impl.Project.ICurrentProjectAccessor</c>, which tracks an ambient
/// async-flow context for background code — this tracks a single,
/// UI-observable piece of state read synchronously when building an
/// <c>ActionContext</c>.
/// </summary>
public sealed class ActiveProjectTracker
{
    /// <summary>Raised when the active project changes.</summary>
    public event Action<ProjectId?>? ActiveProjectChanged;

    /// <summary>The currently active project, or <c>null</c> if none is active.</summary>
    public ProjectId? ActiveProjectId { get; private set; }

    /// <summary>
    /// Sets the active project.
    /// </summary>
    /// <param name="projectId">The project to make active, or <c>null</c> to clear it.</param>
    public void SetActive(ProjectId? projectId)
    {
        if (ActiveProjectId == projectId)
        {
            return;
        }

        ActiveProjectId = projectId;
        ActiveProjectChanged?.Invoke(projectId);
    }
}