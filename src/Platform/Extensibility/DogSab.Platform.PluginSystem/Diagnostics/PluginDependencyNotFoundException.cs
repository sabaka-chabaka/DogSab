using DogSab.Platform.Extensibility.Abstractions.Manifest;

namespace DogSab.Platform.PluginSystem.Diagnostics;

/// <summary>
/// Thrown when a plugin declares a required (non-optional) dependency on
/// another plugin that was not found among the discovered plugins.
/// </summary>
public sealed class PluginDependencyNotFoundException : Exception
{
    /// <summary>The plugin whose required dependency could not be found.</summary>
    public PluginId DependentPluginId { get; }

    /// <summary>The missing dependency's plugin ID.</summary>
    public PluginId MissingDependencyId { get; }

    /// <summary>
    /// Creates a new exception describing a missing required dependency.
    /// </summary>
    /// <param name="dependentPluginId">The plugin whose dependency could not be found.</param>
    /// <param name="missingDependencyId">The missing dependency's plugin ID.</param>
    public PluginDependencyNotFoundException(PluginId dependentPluginId, PluginId missingDependencyId)
        : base($"Plugin '{dependentPluginId}' requires plugin '{missingDependencyId}', which was not found among discovered plugins.")
    {
        DependentPluginId = dependentPluginId;
        MissingDependencyId = missingDependencyId;
    }
}