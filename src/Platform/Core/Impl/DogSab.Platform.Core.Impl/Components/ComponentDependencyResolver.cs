using DogSab.Platform.Core.Abstractions.Components;
using DogSab.Platform.Core.Abstractions.Exceptions;
using DogSab.Platform.Core.Impl.Diagnostics;

namespace DogSab.Platform.Core.Impl.Components;

/// <summary>
/// Resolves the correct initialization order for a component and its transitive
/// dependencies using a topological sort over <see cref="ComponentDependsOnAttribute"/>
/// declarations on implementation types.
/// </summary>
public sealed class ComponentDependencyResolver
{
    /// <summary>
    /// Computes the initialization order for <paramref name="target"/> and all of its
    /// transitive dependencies, such that every dependency appears before the component
    /// that depends on it.
    /// </summary>
    /// <param name="target">The component whose dependency chain should be resolved.</param>
    /// <param name="allDescriptors">The full set of registered component descriptors, keyed by interface type.</param>
    /// <returns>An ordered sequence of descriptors, dependencies first.</returns>
    /// <exception cref="CircularDependencyException">
    /// Thrown if a cycle is detected among the component's dependencies.
    /// </exception>
    /// <exception cref="ComponentNotFoundException">
    /// Thrown if a declared dependency has not been registered.
    /// </exception>
    public IReadOnlyList<ComponentDescriptor> ResolveOrder(
        ComponentDescriptor target,
        IReadOnlyDictionary<Type, ComponentDescriptor> allDescriptors)
    {
        var result = new List<ComponentDescriptor>();
        var visited = new HashSet<Type>();
        var visiting = new HashSet<Type>();

        Visit(target, allDescriptors, visited, visiting, result);

        return result;
    }

    /// <summary>
    /// Recursively visits a component's dependencies in depth-first order,
    /// appending each descriptor to <paramref name="result"/> only after all
    /// of its own dependencies have been appended.
    /// </summary>
    /// <param name="current">The descriptor currently being visited.</param>
    /// <param name="allDescriptors">The full set of registered component descriptors.</param>
    /// <param name="visited">Interface types that have already been fully processed.</param>
    /// <param name="visiting">Interface types currently on the recursion stack, used for cycle detection.</param>
    /// <param name="result">The accumulator list building up the final initialization order.</param>
    private void Visit(
        ComponentDescriptor current,
        IReadOnlyDictionary<Type, ComponentDescriptor> allDescriptors,
        HashSet<Type> visited,
        HashSet<Type> visiting,
        List<ComponentDescriptor> result)
    {
        if (visited.Contains(current.InterfaceType))
        {
            return;
        }

        if (!visiting.Add(current.InterfaceType))
        {
            throw new CircularDependencyException(BuildCyclePath(visiting, current.InterfaceType));
        }

        foreach (var dependencyType in GetDeclaredDependencies(current.ImplementationType))
        {
            if (!allDescriptors.TryGetValue(dependencyType, out var dependencyDescriptor))
            {
                throw new ComponentNotFoundException(dependencyType);
            }

            Visit(dependencyDescriptor, allDescriptors, visited, visiting, result);
        }

        visiting.Remove(current.InterfaceType);
        visited.Add(current.InterfaceType);
        result.Add(current);
    }

    /// <summary>
    /// Reads all <see cref="ComponentDependsOnAttribute"/> declarations from an implementation type.
    /// </summary>
    /// <param name="implementationType">The concrete component implementation type to inspect.</param>
    /// <returns>The interface types this component declares as dependencies.</returns>
    private static IEnumerable<Type> GetDeclaredDependencies(Type implementationType)
    {
        return implementationType
            .GetCustomAttributes(typeof(ComponentDependsOnAttribute), inherit: true)
            .Cast<ComponentDependsOnAttribute>()
            .Select(attr => attr.DependencyInterfaceType);
    }

    /// <summary>
    /// Builds a human-readable description of the dependency cycle for diagnostics.
    /// </summary>
    /// <param name="visiting">The set of interface types currently on the recursion stack.</param>
    /// <param name="closingType">The interface type at which the cycle was detected.</param>
    /// <returns>A string listing the involved types, for use in the exception message.</returns>
    private static string BuildCyclePath(IEnumerable<Type> visiting, Type closingType)
    {
        var names = visiting.Select(t => t.Name).Append(closingType.Name);
        return string.Join(" -> ", names);
    }
}