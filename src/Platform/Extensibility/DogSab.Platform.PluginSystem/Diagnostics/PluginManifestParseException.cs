namespace DogSab.Platform.PluginSystem.Diagnostics;

/// <summary>
/// Thrown when the plugin manifest parser cannot parse a manifest file —
/// either the JSON itself is malformed, or it is well-formed JSON missing a
/// required field or containing an invalid value (e.g. an unparsable version string).
/// </summary>
public sealed class PluginManifestParseException : Exception
{
    /// <summary>The path of the manifest file that failed to parse.</summary>
    public string ManifestPath { get; }

    /// <summary>
    /// Creates a new exception describing a failed manifest parse.
    /// </summary>
    /// <param name="manifestPath">The path of the manifest file that failed to parse.</param>
    /// <param name="reason">A specific description of what was wrong with the manifest.</param>
    /// <param name="inner">The underlying exception, if the failure was caused by one (e.g. a JSON syntax error).</param>
    public PluginManifestParseException(string manifestPath, string reason, Exception? inner = null)
        : base($"Failed to parse plugin manifest at '{manifestPath}': {reason}", inner)
    {
        ManifestPath = manifestPath;
    }
}