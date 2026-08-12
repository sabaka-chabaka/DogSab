using DogSab.Platform.Core.Impl.DependencyResolution;
using DogSab.Platform.ProjectModel.Abstractions.Module;

namespace DogSab.Platform.ProjectModel.Module;

/// <summary>
/// Orders a project's modules so every module appears after all of its
/// dependencies — needed, for example, to compile modules in the correct
/// order or to build a combined reference list. A thin wrapper over the
/// platform's shared <see cref="TopologicalSorter{TNode,TKey}"/>, supplying
/// how to identify a module (<see cref="IModule.Id"/>) and how to find its
/// dependencies (<see cref="IModule.Dependencies"/>) — the traversal
/// algorithm itself is not reimplemented here.
/// </summary>
public sealed class ModuleDependencyResolver
{
    /// <summary>The shared topological sort algorithm this resolver configures for modules.</summary>
    private readonly TopologicalSorter<IModule, ModuleId> _sorter = new();

    /// <summary>
    /// Computes the dependency order for a set of modules.
    /// </summary>
    /// <param name="modules">The modules to order.</param>
    /// <returns>The modules, ordered so every dependency precedes its dependents.</returns>
    /// <exception cref="TopologicalSortCycleException{ModuleId}">Thrown if a cycle is detected among module dependencies.</exception>
    /// <exception cref="TopologicalSortMissingDependencyException{ModuleId}">Thrown if a module depends on a module not present in <paramref name="modules"/>.</exception>
    public IReadOnlyList<IModule> ResolveBuildOrder(IReadOnlyList<IModule> modules)
    {
        return _sorter.Sort(
            modules,
            keySelector: module => module.Id,
            dependencyKeysSelector: module => module.Dependencies.Select(d => d.DependencyModuleId));
    }
}