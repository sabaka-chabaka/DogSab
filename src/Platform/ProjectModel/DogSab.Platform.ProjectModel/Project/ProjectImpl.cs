using DogSab.Platform.ProjectModel.Abstractions.Module;
using DogSab.Platform.ProjectModel.Abstractions.Project;

namespace DogSab.Platform.ProjectModel.Project;

/// <summary>
/// Default implementation of <see cref="IProject"/>. Immutable with respect
/// to its module list — adding or removing a module is modeled as producing
/// a new <see cref="ProjectImpl"/> instance (see <see cref="WithModule"/>/
/// <see cref="WithoutModule"/>) rather than mutating in place, so that
/// <see cref="Events.IProjectModelListener"/> notifications can reliably
/// capture "before" and "after" snapshots without the risk of a listener
/// observing a half-updated project mid-mutation.
/// </summary>
public sealed class ProjectImpl : IProject
{
    private readonly Dictionary<ModuleId, IModule> _modulesById;

    /// <inheritdoc />
    public ProjectId Id { get; }

    /// <inheritdoc />
    public string DisplayName { get; }

    /// <inheritdoc />
    public IReadOnlyList<IModule> Modules { get; }

    /// <summary>
    /// Creates a new project.
    /// </summary>
    /// <param name="id">The project's stable identifier.</param>
    /// <param name="displayName">A human-readable display name.</param>
    /// <param name="modules">The modules that make up this project.</param>
    public ProjectImpl(ProjectId id, string displayName, IReadOnlyList<IModule> modules)
    {
        Id = id;
        DisplayName = displayName;
        Modules = modules;
        _modulesById = modules.ToDictionary(m => m.Id);
    }

    /// <inheritdoc />
    public IModule? FindModule(ModuleId moduleId)
    {
        return _modulesById.TryGetValue(moduleId, out var module) ? module : null;
    }

    /// <summary>
    /// Returns a new project instance with the given module added (or
    /// replacing an existing module with the same ID).
    /// </summary>
    /// <param name="module">The module to add.</param>
    /// <returns>A new <see cref="ProjectImpl"/> reflecting the addition.</returns>
    public ProjectImpl WithModule(IModule module)
    {
        var newModules = Modules.Where(m => m.Id != module.Id).Append(module).ToList();
        return new ProjectImpl(Id, DisplayName, newModules);
    }

    /// <summary>
    /// Returns a new project instance with the given module removed.
    /// </summary>
    /// <param name="moduleId">The identifier of the module to remove.</param>
    /// <returns>A new <see cref="ProjectImpl"/> reflecting the removal.</returns>
    public ProjectImpl WithoutModule(ModuleId moduleId)
    {
        var newModules = Modules.Where(m => m.Id != moduleId).ToList();
        return new ProjectImpl(Id, DisplayName, newModules);
    }
}