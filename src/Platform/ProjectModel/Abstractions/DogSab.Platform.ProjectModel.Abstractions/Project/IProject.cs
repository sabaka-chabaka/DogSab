using DogSab.Platform.ProjectModel.Abstractions.Module;

namespace DogSab.Platform.ProjectModel.Abstractions.Project;

/// <summary>
/// A single project within a solution, containing one or more
/// <see cref="IModule"/> entries. Roughly corresponds to a logical grouping
/// of related modules (e.g. an application and its associated libraries),
/// though the platform imposes no specific meaning beyond "a set of modules
/// that are conceptually one project."
/// </summary>
public interface IProject
{
    /// <summary>The project's stable identifier.</summary>
    ProjectId Id { get; }

    /// <summary>A human-readable display name.</summary>
    string DisplayName { get; }

    /// <summary>The modules that make up this project.</summary>
    IReadOnlyList<IModule> Modules { get; }

    /// <summary>
    /// Looks up a module by its identifier.
    /// </summary>
    /// <param name="moduleId">The module identifier to look up.</param>
    /// <returns>The module, or <c>null</c> if no module with that identifier exists in this project.</returns>
    IModule? FindModule(ModuleId moduleId);
}