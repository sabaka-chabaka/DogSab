using DogSab.Platform.ProjectModel.Abstractions.Module;
using DogSab.Platform.ProjectModel.Abstractions.Project;

namespace DogSab.Platform.ProjectModel.Abstractions.Events;

/// <summary>
/// Listener interface for structural changes to the project model, published
/// on <see cref="ProjectModelTopics.MODEL_CHANGED"/>. Subscribers (e.g. the
/// Project View tool window, or Indexing when a module's content roots
/// change) react to modules and projects being added or removed, without
/// polling the model themselves.
/// </summary>
public interface IProjectModelListener
{
    /// <summary>Called after a project has been added to the solution.</summary>
    /// <param name="project">The project that was added.</param>
    void ProjectAdded(IProject project);

    /// <summary>Called before a project is removed from the solution, while it is still fully valid.</summary>
    /// <param name="project">The project about to be removed.</param>
    void ProjectRemoving(IProject project);

    /// <summary>Called after a module has been added to a project.</summary>
    /// <param name="project">The project the module was added to.</param>
    /// <param name="module">The module that was added.</param>
    void ModuleAdded(IProject project, IModule module);

    /// <summary>Called before a module is removed from a project, while it is still fully valid.</summary>
    /// <param name="project">The project the module is being removed from.</param>
    /// <param name="module">The module about to be removed.</param>
    void ModuleRemoving(IProject project, IModule module);
}