using DogSab.Platform.Extensibility.Abstractions.Compatibility;
using DogSab.Platform.Extensibility.Abstractions.Manifest;
using DogSab.Platform.Extensibility.Abstractions.Sandbox;

namespace DogSab.Platform.PluginSystem.Manifest;

/// <summary>
/// Concrete, immutable implementation of <see cref="IPluginManifest"/>.
/// Constructed exclusively by <see cref="PluginManifestParser"/> once a raw
/// <see cref="PluginManifestJsonModel"/> has been fully validated and its
/// string fields converted into their typed equivalents (<c>PluginId</c>,
/// <c>PluginVersion</c>, <c>VersionRange</c>). By the time an instance of
/// this class exists, the manifest is known to be well-formed — any
/// malformed input has already been rejected as a
/// <see cref="Diagnostics.PluginManifestParseException"/> during parsing.
/// </summary>
internal sealed class PluginManifestImpl : IPluginManifest
{
    /// <inheritdoc />
    public PluginId Id { get; }

    /// <inheritdoc />
    public PluginVersion Version { get; }

    /// <inheritdoc />
    public string DisplayName { get; }

    /// <inheritdoc />
    public string Description { get; }

    /// <inheritdoc />
    public string Author { get; }

    /// <inheritdoc />
    public VersionRange CompatiblePlatformVersionRange { get; }

    /// <inheritdoc />
    public IReadOnlyList<PluginDependencyDescriptor> Dependencies { get; }

    /// <inheritdoc />
    public IReadOnlyList<ExtensionDeclaration> Extensions { get; }

    /// <inheritdoc />
    public string MainAssemblyFileName { get; }

    /// <inheritdoc />
    public IReadOnlyList<PluginPermission> RequestedPermissions { get; }

    /// <summary>
    /// Creates a new, fully-validated plugin manifest instance.
    /// </summary>
    /// <param name="id">The plugin's unique identifier.</param>
    /// <param name="version">The plugin's own version.</param>
    /// <param name="displayName">A human-readable name shown in the Plugin Manager UI.</param>
    /// <param name="description">A short description of what the plugin does.</param>
    /// <param name="author">The plugin's author or publisher.</param>
    /// <param name="compatiblePlatformVersionRange">The range of platform versions this plugin is compatible with.</param>
    /// <param name="dependencies">Every other plugin this plugin depends on.</param>
    /// <param name="extensions">Every extension point registration this plugin declares.</param>
    /// <param name="mainAssemblyFileName">The file name of the plugin's main assembly.</param>
    /// <param name="requestedPermissions">The permissions this plugin declares it needs.</param>
    public PluginManifestImpl(
        PluginId id,
        PluginVersion version,
        string displayName,
        string description,
        string author,
        VersionRange compatiblePlatformVersionRange,
        IReadOnlyList<PluginDependencyDescriptor> dependencies,
        IReadOnlyList<ExtensionDeclaration> extensions,
        string mainAssemblyFileName,
        IReadOnlyList<PluginPermission> requestedPermissions)
    {
        Id = id;
        Version = version;
        DisplayName = displayName;
        Description = description;
        Author = author;
        CompatiblePlatformVersionRange = compatiblePlatformVersionRange;
        Dependencies = dependencies;
        Extensions = extensions;
        MainAssemblyFileName = mainAssemblyFileName;
        RequestedPermissions = requestedPermissions;
    }
}