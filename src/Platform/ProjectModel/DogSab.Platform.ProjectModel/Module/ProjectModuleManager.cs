using DogSab.Platform.Core.Abstractions.Messaging;
using DogSab.Platform.ProjectModel.Abstractions.Events;
using DogSab.Platform.ProjectModel.Abstractions.Module;
using DogSab.Platform.ProjectModel.Abstractions.Project;
using DogSab.Platform.ProjectModel.Abstractions.Solution;
using DogSab.Platform.ProjectModel.Project;
using DogSab.Platform.ProjectModel.Solution;

namespace DogSab.Platform.ProjectModel.Module;

public sealed class ProjectModelManager
{
    private readonly IMessageBus _messageBus;
    private ISolution _currentSolution;

    public ProjectModelManager(ISolution initialSolution, IMessageBus messageBus)
    {
        _currentSolution = initialSolution;
        _messageBus = messageBus;
    }

    public ISolution CurrentSolution => _currentSolution;

    public void AddModule(ProjectId projectId, IModule module)
    {
        var solution = RequireOwnSolution();
        var project = RequireOwnProject(solution, projectId);

        var updatedProject = project.WithModule(module);
        _currentSolution = solution.WithProject(updatedProject);

        _messageBus.Publisher(ProjectModelTopics.MODEL_CHANGED).ModuleAdded(updatedProject, module);
    }

    public void RemoveModule(ProjectId projectId, ModuleId moduleId)
    {
        var solution = RequireOwnSolution();
        var project = RequireOwnProject(solution, projectId);

        var module = project.FindModule(moduleId)
            ?? throw new InvalidOperationException($"Module '{moduleId}' does not exist in project '{projectId}'.");

        _messageBus.Publisher(ProjectModelTopics.MODEL_CHANGED).ModuleRemoving(project, module);

        var updatedProject = project.WithoutModule(moduleId);
        _currentSolution = solution.WithProject(updatedProject);
    }

    /// <summary>
    /// Verifies the currently held solution is a <see cref="SolutionImpl"/>,
    /// as required to call its <c>WithProject</c> mutation-producing method.
    /// </summary>
    private SolutionImpl RequireOwnSolution()
    {
        return _currentSolution as SolutionImpl
            ?? throw new InvalidOperationException(
                $"{nameof(ProjectModelManager)} requires the held solution to be a {nameof(SolutionImpl)}, " +
                $"but it is a '{_currentSolution.GetType().FullName}'.");
    }

    /// <summary>
    /// Looks up a project by ID and verifies it is a <see cref="ProjectImpl"/>,
    /// as required to call its <c>WithModule</c>/<c>WithoutModule</c> methods.
    /// </summary>
    private static ProjectImpl RequireOwnProject(ISolution solution, ProjectId projectId)
    {
        var project = solution.FindProject(projectId)
            ?? throw new InvalidOperationException($"Project '{projectId}' does not exist in the current solution.");

        return project as ProjectImpl
            ?? throw new InvalidOperationException(
                $"{nameof(ProjectModelManager)} requires projects to be {nameof(ProjectImpl)}, " +
                $"but project '{projectId}' is a '{project.GetType().FullName}'.");
    }
}