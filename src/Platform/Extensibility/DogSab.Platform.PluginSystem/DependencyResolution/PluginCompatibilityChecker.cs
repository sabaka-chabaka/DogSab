using DogSab.Platform.Extensibility.Abstractions.Compatibility;
using DogSab.Platform.Extensibility.Abstractions.Manifest;

namespace DogSab.Platform.PluginSystem.DependencyResolution;

/// <summary>
/// Checks a plugin's declared version compatibility requirements against the
/// running platform's version and its dependencies' actual versions, using
/// <see cref="VersionRange.Contains"/> from Extensibility.Abstractions.
/// </summary>
public sealed class PluginCompatibilityChecker
{
    /// <summary>
    /// Checks whether a plugin's declared platform compatibility range is
    /// satisfied by the running platform's version.
    /// </summary>
    /// <param name="manifest">The plugin manifest to check.</param>
    /// <param name="runningPlatformVersion">The version of the currently running platform.</param>
    /// <returns><c>true</c> if compatible; otherwise <c>false</c>.</returns>
    public bool IsPlatformCompatible(IPluginManifest manifest, PluginVersion runningPlatformVersion)
    {
        return manifest.CompatiblePlatformVersionRange.Contains(runningPlatformVersion);
    }

    /// <summary>
    /// Checks whether an installed dependency's actual version satisfies a
    /// plugin's declared acceptable range for that dependency.
    /// </summary>
    /// <param name="dependency">The dependency descriptor declaring the acceptable range.</param>
    /// <param name="actualDependencyVersion">The actual version of the installed dependency plugin.</param>
    /// <returns><c>true</c> if compatible; otherwise <c>false</c>.</returns>
    public bool IsDependencyCompatible(PluginDependencyDescriptor dependency, PluginVersion actualDependencyVersion)
    {
        return dependency.AcceptableVersionRange.Contains(actualDependencyVersion);
    }
}