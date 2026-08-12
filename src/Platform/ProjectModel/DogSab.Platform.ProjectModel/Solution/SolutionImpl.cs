using DogSab.Platform.ProjectModel.Abstractions.Project;
using DogSab.Platform.ProjectModel.Abstractions.Solution;

namespace DogSab.Platform.ProjectModel.Solution;

/// <summary>
/// Default implementation of <see cref="ISolution"/>. Immutable with respect
/// to its project list, following the same "produce a new instance" pattern
/// as <see cref="Project.ProjectImpl"/>, for the same reason — reliable
/// before/after snapshots around <see cref="Events.IProjectModelListener"/> notifications.
/// </summary>
public sealed class SolutionImpl : ISolution
{
    private readonly Dictionary<ProjectId, IProject> _projectsById;

    /// <inheritdoc />
    public SolutionId Id { get; }

    /// <inheritdoc />
    public string DisplayName { get; }

    /// <inheritdoc />
    public IReadOnlyList<IProject> Projects { get; }

    /// <summary>
    /// Creates a new solution.
    /// </summary>
    /// <param name="id">The solution's stable identifier.</param>
    /// <param name="displayName">A human-readable display name.</param>
    /// <param name="projects">The projects contained in this solution.</param>
    public SolutionImpl(SolutionId id, string displayName, IReadOnlyList<IProject> projects)
    {
        Id = id;
        DisplayName = displayName;
        Projects = projects;
        _projectsById = projects.ToDictionary(p => p.Id);
    }

    /// <inheritdoc />
    public IProject? FindProject(ProjectId projectId)
    {
        return _projectsById.TryGetValue(projectId, out var project) ? project : null;
    }

    /// <summary>
    /// Returns a new solution instance with the given project added (or
    /// replacing an existing project with the same ID).
    /// </summary>
    /// <param name="project">The project to add.</param>
    /// <returns>A new <see cref="SolutionImpl"/> reflecting the addition.</returns>
    public SolutionImpl WithProject(IProject project)
    {
        var newProjects = Projects.Where(p => p.Id != project.Id).Append(project).ToList();
        return new SolutionImpl(Id, DisplayName, newProjects);
    }

    /// <summary>
    /// Returns a new solution instance with the given project removed.
    /// </summary>
    /// <param name="projectId">The identifier of the project to remove.</param>
    /// <returns>A new <see cref="SolutionImpl"/> reflecting the removal.</returns>
    public SolutionImpl WithoutProject(ProjectId projectId)
    {
        var newProjects = Projects.Where(p => p.Id != projectId).ToList();
        return new SolutionImpl(Id, DisplayName, newProjects);
    }
}