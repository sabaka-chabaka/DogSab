using DogSab.Platform.Core.Abstractions.Messaging;
using DogSab.Platform.ProjectModel.Abstractions.Events;
using DogSab.Platform.ProjectModel.Abstractions.Module;
using DogSab.Platform.ProjectModel.Abstractions.Project;
using DogSab.Platform.ProjectModel.Abstractions.Solution;
using DogSab.Platform.ProjectModel.Project;
using DogSab.Platform.ProjectModel.Solution;

namespace DogSab.Platform.ProjectModel.Module;

/// <summary>
/// Holds the currently loaded solution and is the single point of mutation
/// for the project model: every add/remove operation replaces the held
/// <see cref="ISolution"/> reference with a new immutable instance (via
/// <see cref="SolutionImpl.WithProject"/>/<see cref="ProjectImpl.WithModule"/>)
/// and publishes the corresponding <see cref="IProjectModelListener"/>
/// notification on <see cref="ProjectModelTopics.MODEL_CHANGED"/>. Platform
/// code should mutate the project model only through this manager, never by
/// constructing a new <see cref="SolutionImpl"/> directly, so every
/// structural change is reliably observed by subscribers.
/// </summary>
public sealed class ProjectModelManager
{
    private readonly IMessageBus _messageBus;
    private ISolution _currentSolution;

    /// <summary>
    /// Creates a new project model manager holding an initial solution.
    /// </summary>
    /// <param name="initialSolution">The solution to start with, typically freshly loaded via <see cref="Persistence.XmlProjectModelPersistence"/>.</param>
    /// <param name="messageBus">The message bus to publish model change notifications on.</param>
    public ProjectModelManager(ISolution initialSolution, IMessageBus messageBus)
    {
        _currentSolution = initialSolution;
        _messageBus = messageBus;
    }

    /// <summary>The currently held solution structure.</summary>
    public ISolution CurrentSolution => _currentSolution;

    /// <summary>
    /// Adds a module to a project, replacing the held solution with an
    /// updated immutable copy and notifying subscribers.
    /// </summary>
    /// <param name="projectId">The project to add the module to.</param>
    /// <param name="module">The module to add.</param>
    public void AddModule(ProjectId projectId, IModule module)
    {
        var project = (ProjectImpl)_currentSolution.FindProject(projectId)!;
        var updatedProject = project.WithModule(module);

        _currentSolution = ((SolutionImpl)_currentSolution).WithProject(updatedProject);

        _messageBus.Publisher(ProjectModelTopics.MODEL_CHANGED).ModuleAdded(updatedProject, module);
    }

    /// <summary>
    /// Removes a module from a project, replacing the held solution with an
    /// updated immutable copy and notifying subscribers before the removal takes effect.
    /// </summary>
    /// <param name="projectId">The project to remove the module from.</param>
    /// <param name="moduleId">The module to remove.</param>
    public void RemoveModule(ProjectId projectId, ModuleId moduleId)
    {
        var project = (ProjectImpl)_currentSolution.FindProject(projectId)!;
        var module = project.FindModule(moduleId)!;

        _messageBus.Publisher(ProjectModelTopics.MODEL_CHANGED).ModuleRemoving(project, module);

        var updatedProject = project.WithoutModule(moduleId);
        _currentSolution = ((SolutionImpl)_currentSolution).WithProject(updatedProject);
    }
}