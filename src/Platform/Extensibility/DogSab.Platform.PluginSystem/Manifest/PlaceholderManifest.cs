using DogSab.Platform.Extensibility.Abstractions.Compatibility;
using DogSab.Platform.Extensibility.Abstractions.Manifest;
using DogSab.Platform.Extensibility.Abstractions.Sandbox;

namespace DogSab.Platform.PluginSystem.Manifest;

/// <summary>
/// Produces a minimal, non-functional <see cref="IPluginManifest"/> used only
/// when a plugin's real manifest could not be parsed at all, so that a
/// <see cref="Loading.PluginDescriptorImpl"/> can still be constructed and
/// shown in diagnostics — a plugin directory with no valid manifest must
/// still be identifiable and listed as failed, not silently dropped from
/// every list that expects an <see cref="IPluginManifest"/> to be present.
/// </summary>
internal static class PlaceholderManifest
{
    /// <summary>
    /// Creates a placeholder manifest identified by a plugin's directory name,
    /// since no real ID is available from an unparsable manifest.
    /// </summary>
    /// <param name="directoryName">The plugin's directory name, used as a stand-in identifier.</param>
    /// <returns>A minimal, non-functional manifest.</returns>
    public static IPluginManifest ForUnparsableDirectory(string directoryName)
    {
        return new PluginManifestImpl(
            new PluginId($"unparsable.{directoryName}"),
            new PluginVersion(0, 0, 0),
            directoryName,
            "This plugin's manifest could not be parsed.",
            author: string.Empty,
            VersionRange.Any(),
            dependencies: System.Array.Empty<PluginDependencyDescriptor>(),
            extensions: System.Array.Empty<ExtensionDeclaration>(),
            mainAssemblyFileName: string.Empty,
            requestedPermissions: System.Array.Empty<PluginPermission>());
    }
}