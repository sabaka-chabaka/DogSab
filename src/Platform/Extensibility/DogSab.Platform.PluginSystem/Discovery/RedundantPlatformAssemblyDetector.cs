namespace DogSab.Platform.PluginSystem.Discovery;

/// <summary>
/// Scans a plugin's directory for bundled copies of platform contract
/// assemblies (anything named <c>DogSab.Platform.*.dll</c>). Such copies are
/// harmless at runtime — <see cref="Loading.PluginAssemblyLoadContext.Load"/>
/// refuses to load them, always falling back to the platform's own copy in
/// the default context — but indicate the plugin's build did not correctly
/// exclude platform references from its output, bloating the distributed
/// plugin package for no benefit. Reported as a non-fatal warning rather than
/// a load failure.
/// </summary>
public sealed class RedundantPlatformAssemblyDetector
{
    /// <summary>The prefix identifying platform contract assemblies, matching <see cref="Loading.PluginAssemblyLoadContext"/>'s own guard.</summary>
    private const string PlatformAssemblyPrefix = "DogSab.Platform.";

    /// <summary>
    /// Scans a plugin's directory for redundant platform assembly copies.
    /// </summary>
    /// <param name="pluginDirectory">The plugin's directory to scan.</param>
    /// <returns>
    /// A human-readable warning message per redundant assembly found, or an
    /// empty list if none were found.
    /// </returns>
    public IReadOnlyList<string> Scan(string pluginDirectory)
    {
        if (!Directory.Exists(pluginDirectory))
        {
            return Array.Empty<string>();
        }

        return Directory.EnumerateFiles(pluginDirectory, "*.dll", SearchOption.TopDirectoryOnly)
            .Where(path => Path.GetFileNameWithoutExtension(path).StartsWith(PlatformAssemblyPrefix, StringComparison.Ordinal))
            .Select(path => BuildWarningMessage(Path.GetFileName(path)))
            .ToList();
    }

    /// <summary>
    /// Builds a human-readable warning message for a single redundant assembly file found.
    /// </summary>
    /// <param name="fileName">The name of the redundant assembly file.</param>
    /// <returns>The warning message.</returns>
    private static string BuildWarningMessage(string fileName)
    {
        return $"Plugin bundles '{fileName}', a platform assembly that will be ignored at runtime " +
               $"(the platform's own copy is always used instead). Exclude it from the plugin's build " +
               $"output — e.g. via <PrivateAssets>all</PrivateAssets> on the corresponding ProjectReference.";
    }
}