using DogSab.Platform.Extensibility.Abstractions.Manifest;

namespace DogSab.Platform.Extensibility.Abstractions.Compatibility;

/// <summary>
/// Thrown when a plugin cannot be loaded because its declared platform
/// compatibility range or a required dependency's version range is not
/// satisfied by what is actually installed/running. Carries enough detail for
/// the Plugin Manager UI to explain the failure to the user without needing
/// to re-derive it from a generic exception message.
/// </summary>
public sealed class PluginIncompatibleException : Exception
{
    /// <summary>The plugin that could not be loaded due to an incompatibility.</summary>
    public PluginId PluginId { get; }

    /// <summary>
    /// The identifier of the thing the plugin was incompatible with: either
    /// the platform itself (conventionally reported as <c>"platform"</c>) or
    /// another plugin's <see cref="Manifest.PluginId"/> value.
    /// </summary>
    public string IncompatibleWithId { get; }

    /// <summary>The version range the plugin required.</summary>
    public VersionRange RequiredRange { get; }

    /// <summary>The actual version that failed to satisfy <see cref="RequiredRange"/>.</summary>
    public PluginVersion ActualVersion { get; }

    /// <summary>
    /// Creates a new exception describing a plugin compatibility failure.
    /// </summary>
    /// <param name="pluginId">The plugin that could not be loaded.</param>
    /// <param name="incompatibleWithId">The identifier of the platform or plugin the incompatibility is against.</param>
    /// <param name="requiredRange">The version range the plugin required.</param>
    /// <param name="actualVersion">The actual version that failed to satisfy the range.</param>
    public PluginIncompatibleException(
        PluginId pluginId,
        string incompatibleWithId,
        VersionRange requiredRange,
        PluginVersion actualVersion)
        : base(BuildMessage(pluginId, incompatibleWithId, requiredRange, actualVersion))
    {
        PluginId = pluginId;
        IncompatibleWithId = incompatibleWithId;
        RequiredRange = requiredRange;
        ActualVersion = actualVersion;
    }

    /// <summary>
    /// Builds a human-readable message describing the version mismatch, for
    /// use as the exception's top-level <see cref="Exception.Message"/>.
    /// </summary>
    /// <param name="pluginId">The plugin that could not be loaded.</param>
    /// <param name="incompatibleWithId">The identifier of the platform or plugin the incompatibility is against.</param>
    /// <param name="requiredRange">The version range the plugin required.</param>
    /// <param name="actualVersion">The actual version that failed to satisfy the range.</param>
    /// <returns>A descriptive message for the exception.</returns>
    private static string BuildMessage(
        PluginId pluginId,
        string incompatibleWithId,
        VersionRange requiredRange,
        PluginVersion actualVersion)
    {
        return $"Plugin '{pluginId}' requires '{incompatibleWithId}' version {requiredRange}, " +
               $"but the installed/running version is {actualVersion}.";
    }
}