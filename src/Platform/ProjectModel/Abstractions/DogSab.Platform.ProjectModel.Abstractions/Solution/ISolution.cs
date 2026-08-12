using DogSab.Platform.ProjectModel.Abstractions.Project;

namespace DogSab.Platform.ProjectModel.Abstractions.Solution;

/// <summary>
/// The top-level container for a project structure: holds one or more
/// <see cref="IProject"/> entries, supporting workspaces where multiple
/// related projects are open together (mirroring how a single DogSab session
/// can have multiple <c>ProjectSession</c> entries open at once — though this
/// interface represents the structural model, not the runtime session state
/// managed by <c>Core.Application.ProjectLifecycle</c>).
/// </summary>
public interface ISolution
{
    /// <summary>The solution's stable identifier.</summary>
    SolutionId Id { get; }

    /// <summary>A human-readable display name.</summary>
    string DisplayName { get; }

    /// <summary>The projects contained in this solution.</summary>
    IReadOnlyList<IProject> Projects { get; }

    /// <summary>
    /// Looks up a project by its identifier.
    /// </summary>
    /// <param name="projectId">The project identifier to look up.</param>
    /// <returns>The project, or <c>null</c> if no project with that identifier exists in this solution.</returns>
    IProject? FindProject(ProjectId projectId);
}