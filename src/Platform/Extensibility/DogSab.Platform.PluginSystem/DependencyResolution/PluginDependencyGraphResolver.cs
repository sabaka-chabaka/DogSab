using DogSab.Platform.Extensibility.Abstractions.Loading;
using DogSab.Platform.Extensibility.Abstractions.Manifest;
using DogSab.Platform.PluginSystem.Diagnostics;

namespace DogSab.Platform.PluginSystem.DependencyResolution;

/// <summary>
/// Orders a set of discovered plugins so that every plugin appears after all
/// of its declared dependencies, using the same depth-first, post-order
/// traversal pattern as <c>ComponentDependencyResolver</c> in Core.Impl —
/// visit each dependency first, then append the plugin itself, tracking
/// in-progress nodes to detect cycles instead of recursing infinitely.
/// Optional dependencies (<see cref="PluginDependencyDescriptor.IsOptional"/>)
/// that are missing from the input set are skipped rather than treated as an error.
/// </summary>
public sealed class PluginDependencyGraphResolver
{
    /// <summary>
    /// Computes the load order for a set of discovered plugin descriptors.
    /// </summary>
    /// <param name="descriptors">The plugins to order, as returned by discovery.</param>
    /// <returns>The same descriptors, ordered so dependencies precede dependents.</returns>
    /// <exception cref="PluginCircularDependencyException">Thrown if a cycle is detected among required dependencies.</exception>
    /// <exception cref="PluginDependencyNotFoundException">Thrown if a required (non-optional) dependency is missing from <paramref name="descriptors"/>.</exception>
    public IReadOnlyList<IPluginDescriptor> ResolveLoadOrder(IReadOnlyList<IPluginDescriptor> descriptors)
    {
        var byId = descriptors.ToDictionary(d => d.Manifest.Id);
        var result = new List<IPluginDescriptor>();
        var visited = new HashSet<PluginId>();
        var visiting = new HashSet<PluginId>();

        foreach (var descriptor in descriptors)
        {
            Visit(descriptor, byId, visited, visiting, result);
        }

        return result;
    }

    /// <summary>
    /// Recursively visits a plugin's required dependencies before appending
    /// the plugin itself to <paramref name="result"/>, mirroring
    /// <c>ComponentDependencyResolver.Visit</c>.
    /// </summary>
    /// <param name="current">The plugin currently being visited.</param>
    /// <param name="byId">All discovered plugins, keyed by ID, for dependency lookup.</param>
    /// <param name="visited">Plugin IDs already fully processed.</param>
    /// <param name="visiting">Plugin IDs currently on the recursion stack, for cycle detection.</param>
    /// <param name="result">The accumulator list building up the final load order.</param>
    private static void Visit(
        IPluginDescriptor current,
        IReadOnlyDictionary<PluginId, IPluginDescriptor> byId,
        HashSet<PluginId> visited,
        HashSet<PluginId> visiting,
        List<IPluginDescriptor> result)
    {
        var currentId = current.Manifest.Id;

        if (visited.Contains(currentId))
        {
            return;
        }

        if (!visiting.Add(currentId))
        {
            throw new PluginCircularDependencyException(BuildCyclePath(visiting, currentId));
        }

        foreach (var dependency in current.Manifest.Dependencies)
        {
            if (!byId.TryGetValue(dependency.DependencyPluginId, out var dependencyDescriptor))
            {
                if (dependency.IsOptional)
                {
                    continue;
                }

                throw new PluginDependencyNotFoundException(currentId, dependency.DependencyPluginId);
            }

            Visit(dependencyDescriptor, byId, visited, visiting, result);
        }

        visiting.Remove(currentId);
        visited.Add(currentId);
        result.Add(current);
    }

    /// <summary>
    /// Builds a human-readable description of the dependency cycle for diagnostics.
    /// </summary>
    /// <param name="visiting">The plugin IDs currently on the recursion stack.</param>
    /// <param name="closingId">The plugin ID at which the cycle was detected.</param>
    /// <returns>A string listing the involved plugins, for use in the exception message.</returns>
    private static string BuildCyclePath(IEnumerable<PluginId> visiting, PluginId closingId)
    {
        var ids = visiting.Select(id => id.Value).Append(closingId.Value);
        return string.Join(" -> ", ids);
    }
}