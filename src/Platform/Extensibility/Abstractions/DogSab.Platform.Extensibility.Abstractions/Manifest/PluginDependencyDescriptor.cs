using DogSab.Platform.Extensibility.Abstractions.Compatibility;

namespace DogSab.Platform.Extensibility.Abstractions.Manifest;

/// <summary>
/// A single dependency declaration from a plugin's manifest: which other
/// plugin depends on, what version range is acceptable, and whether the
/// dependency is required for the plugin to load at all or merely enables
/// optional functionality when present.
/// </summary>
public readonly struct PluginDependencyDescriptor
{
    /// <summary>The identifier of the plugin beind depended on.</summary>
    public PluginId DependencyPluginId { get; }
    
    /// <summary>The range of versions of the dependency that are acceptable.</summary>
    public VersionRange AcceptableVersionRange { get; }
    
    /// <summary>
    /// Whether this dependency is optional: if <c>true</c> and the dependency
    /// is missing or incompatible, the depending plugin still loads, but any
    /// of its extensions that rely on the dependency's types may fail at
    /// runtime rather than at load time. If <c>false</c>, a missing or
    /// incompatible dependency prevents the depending plugin for loading at all.
    /// </summary>
    public bool IsOptional { get; }
    
    /// <summary>
    /// Creates a new dependency descriptor.
    /// </summary>
    /// <param name="dependencyPluginId">The identifier of the plugin being depended on.</param>
    /// <param name="acceptableVersionRange">The range of versions of the dependency that are acceptable.</param>
    /// <param name="isOptional">Whether the dependency is optional. Defaults to <c>false</c> (required).</param>
    public PluginDependencyDescriptor(PluginId dependencyPluginId, VersionRange acceptableVersionRange, bool isOptional = false)
    {
        DependencyPluginId = dependencyPluginId;
        AcceptableVersionRange = acceptableVersionRange;
        IsOptional = isOptional;
    }

    /// <inheritdoc />
    public override string ToString() =>
        $"{DependencyPluginId} {AcceptableVersionRange}" + (IsOptional ? " (optional)" : string.Empty);
}