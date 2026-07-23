namespace DogSab.Platform.Extensibility.Abstractions.Manifest;

/// <summary>
/// The parsed contents of a plugin's manifest file (e.g. <c>plugin.json</c>):
/// its identity, version, dependencies on other plugins, and the extension
/// points it registers implementations against. Produced by a manifest parser
/// (implemented in <c>DogSab.Platform.PluginSystem</c>, not here) before the
/// plugin's assembly is ever loaded, so dependency and compatibility checks
/// can happen without executing any of the plugin's code.
/// </summary>
public interface IPluginManifest
{
    /// <summary>The plugin's unique identifier.</summary>
    PluginId Id { get; }

    /// <summary>The plugin's own version.</summary>
    PluginVersion Version { get; }

    /// <summary>A human-readable name shown in the Plugin Manager UI.</summary>
    string DisplayName { get; }

    /// <summary>A short description of what the plugin does, shown in the Plugin Manager UI.</summary>
    string Description { get; }

    /// <summary>The plugin's author or publisher, shown in the Plugin Manager UI.</summary>
    string Author { get; }

    /// <summary>
    /// The range of platform versions this plugin declares itself compatible
    /// with. Checked against the running platform's own version before the
    /// plugin is loaded at all.
    /// </summary>
    Compatibility.VersionRange CompatiblePlatformVersionRange { get; }

    /// <summary>Every other plugin this plugin depends on, required or optional.</summary>
    IReadOnlyList<PluginDependencyDescriptor> Dependencies { get; }

    /// <summary>Every extension point registration this plugin declares.</summary>
    IReadOnlyList<ExtensionDeclaration> Extensions { get; }

    /// <summary>
    /// The file name, relative to the plugin's own directory, of the main
    /// assembly containing the plugin's implementation classes referenced by
    /// <see cref="Extensions"/>.
    /// </summary>
    string MainAssemblyFileName { get; }
}