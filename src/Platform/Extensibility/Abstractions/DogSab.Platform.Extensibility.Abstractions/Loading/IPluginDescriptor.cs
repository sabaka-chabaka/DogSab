using DogSab.Platform.Extensibility.Abstractions.Manifest;

namespace DogSab.Platform.Extensibility.Abstractions.Loading;

/// <summary>
/// Represents a single discovered plugin and its current position in the
/// loading lifecycle: its parsed manifest, the directory it was discovered in,
/// and its <see cref="PluginLoadState"/>. Returned by <see cref="IPluginLoader"/>
/// for every plugin found during discovery, regardless of whether it ultimately
/// loaded successfully — so the Plugin Manager UI can list and explain failed
/// or disabled plugins, not just successfully loaded ones.
/// </summary>
public interface IPluginDescriptor
{
    /// <summary>The plugin's parsed manifest.</summary>
    IPluginManifest Manifest { get; }

    /// <summary>The absolute path to the directory this plugin was discovered in.</summary>
    string PluginDirectory { get; }

    /// <summary>The plugin's current position in the loading lifecycle.</summary>
    PluginLoadState State { get; }

    /// <summary>
    /// A human-readable explanation of why loading failed, populated only when
    /// <see cref="State"/> is <see cref="PluginLoadState.Failed"/>; <c>null</c> otherwise.
    /// </summary>
    string? FailureReason { get; }
}