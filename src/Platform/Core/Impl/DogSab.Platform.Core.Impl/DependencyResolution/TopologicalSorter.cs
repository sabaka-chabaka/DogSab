namespace DogSab.Platform.Core.Impl.DependencyResolution;

/// <summary>
/// A generic, reusable topological sort: orders a set of nodes so that every
/// node appears after all of its dependencies, using the same depth-first,
/// post-order traversal with in-progress tracking for cycle detection that
/// was independently derived (by hand, node by node) for tasks, then applied
/// to platform components and plugin manifests. Callers supply how to
/// identify a node (<paramref name="keySelector"/> at call time) and how to
/// find its dependencies (<paramref name="dependenciesSelector"/>), since
/// those differ per use case (a C# attribute for components, a manifest field
/// for plugins, a module reference list for project modules) while the
/// traversal algorithm itself does not.
/// </summary>
public sealed class TopologicalSorter<TNode, TKey> where TKey : notnull
{
    /// <summary>
    /// Computes the dependency order for a set of nodes.
    /// </summary>
    /// <param name="nodes">The nodes to order.</param>
    /// <param name="keySelector">Extracts a node's unique key, used for lookup and cycle detection.</param>
    /// <param name="dependencyKeysSelector">Extracts the keys of a node's direct dependencies.</param>
    /// <returns>The nodes, ordered so every dependency precedes its dependents.</returns>
    /// <exception cref="TopologicalSortCycleException{TKey}">Thrown if a cycle is detected among dependencies.</exception>
    /// <exception cref="TopologicalSortMissingDependencyException{TKey}">Thrown if a node references a dependency key not present in <paramref name="nodes"/>.</exception>
    public IReadOnlyList<TNode> Sort(
        IReadOnlyList<TNode> nodes,
        Func<TNode, TKey> keySelector,
        Func<TNode, IEnumerable<TKey>> dependencyKeysSelector)
    {
        var byKey = nodes.ToDictionary(keySelector);
        var result = new List<TNode>();
        var visited = new HashSet<TKey>();
        var visiting = new HashSet<TKey>();

        foreach (var node in nodes)
        {
            Visit(node, byKey, keySelector, dependencyKeysSelector, visited, visiting, result);
        }
        
        return result;
    }
    
    /// <summary>
    /// Recursively visits a node's dependencies before appending the node
    /// itself to <paramref name="result"/> — the same Visit/inProgress/visited
    /// pattern used throughout the platform's other dependency resolvers.
    /// </summary>
    private void Visit(
        TNode current,
        IReadOnlyDictionary<TKey, TNode> byKey,
        Func<TNode, TKey> keySelector,
        Func<TNode, IEnumerable<TKey>> dependencyKeysSelector,
        HashSet<TKey> visited,
        HashSet<TKey> visiting,
        List<TNode> result)
    {
        var currentKey = keySelector(current);

        if (visited.Contains(currentKey))
        {
            return;
        }

        if (!visiting.Add(currentKey))
        {
            throw new TopologicalSortCycleException(BuildCyclePath(visiting, currentKey));
        }

        foreach (var dependencyKey in dependencyKeysSelector(current))
        {
            if (!byKey.TryGetValue(dependencyKey, out var dependencyNode))
            {
                throw new TopologicalSortMissingDependencyException(currentKey!.ToString()!, dependencyKey!.ToString()!);
            }

            Visit(dependencyNode, byKey, keySelector, dependencyKeysSelector, visited, visiting, result);
        }

        visiting.Remove(currentKey);
        visited.Add(currentKey);
        result.Add(current);
    }

    /// <summary>Builds a human-readable description of the dependency cycle for diagnostics.</summary>
    private static string BuildCyclePath(IEnumerable<TKey> visiting, TKey closingKey)
    {
        var keys = visiting.Select(k => k!.ToString()).Append(closingKey!.ToString());
        return string.Join(" -> ", keys);
    }
}

/// <summary>
/// Thrown when <see cref="TopologicalSorter{TNode,TKey}"/> detects a cycle
/// among dependencies. Non-generic (unlike an earlier version of this
/// exception), so callers can catch it without needing to know the
/// resolver's specific TKey type at the catch site.
/// </summary>
public sealed class TopologicalSortCycleException : Exception
{
    public TopologicalSortCycleException(string cyclePath)
        : base($"Circular dependency detected: {cyclePath}")
    {
    }
}

/// <summary>
/// Thrown when a node references a dependency key not present among the
/// nodes being sorted. Non-generic for the same reason as
/// <see cref="TopologicalSortCycleException"/>.
/// </summary>
public sealed class TopologicalSortMissingDependencyException : Exception
{
    public string DependentKey { get; }
    public string MissingDependencyKey { get; }

    public TopologicalSortMissingDependencyException(string dependentKey, string missingDependencyKey)
        : base($"Node '{dependentKey}' depends on '{missingDependencyKey}', which was not found among the nodes being sorted.")
    {
        DependentKey = dependentKey;
        MissingDependencyKey = missingDependencyKey;
    }
}