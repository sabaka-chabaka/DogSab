using DogSab.Platform.Core.Abstractions.Logging;
using DogSab.Platform.Extensibility.Abstractions.Loading;
using DogSab.Platform.PluginSystem.Loading;
using DogSab.Platform.PluginSystem.Manifest;

namespace DogSab.Platform.PluginSystem.Discovery;

/// <summary>
/// Scans a plugins root directory for subdirectories containing a valid
/// <c>plugin.json</c> manifest, parsing each one without loading any plugin
/// assemblies. A subdirectory whose manifest fails to parse is still reported
/// — as a descriptor in <see cref="PluginLoadState.Failed"/> — rather than
/// silently skipped, so the Plugin Manager UI can show the user exactly which
/// plugin is broken and why.
/// </summary>
public sealed class PluginDiscoveryService
{
    /// <summary>The conventional file name for a plugin's manifest within its directory.</summary>
    private const string ManifestFileName = "plugin.json";

    /// <summary>Parses each discovered manifest file.</summary>
    private readonly PluginManifestParser _manifestParser;

    /// <summary>Detects bundled copies of platform assemblies, reported as non-fatal warnings.</summary>
    private readonly RedundantPlatformAssemblyDetector _redundantAssemblyDetector;

    /// <summary>Logger used to report discovery progress and per-plugin failures.</summary>
    private readonly ILogger _logger;

    /// <summary>
    /// Creates a new plugin discovery service.
    /// </summary>
    /// <param name="manifestParser">Parser used to read each discovered plugin's manifest.</param>
    /// <param name="redundantAssemblyDetector">Detector used to flag bundled platform assembly copies.</param>
    /// <param name="loggerFactory">Factory used to obtain a logger scoped to this service.</param>
    public PluginDiscoveryService(
        PluginManifestParser manifestParser,
        RedundantPlatformAssemblyDetector redundantAssemblyDetector,
        ILoggerFactory loggerFactory)
    {
        _manifestParser = manifestParser;
        _redundantAssemblyDetector = redundantAssemblyDetector;
        _logger = loggerFactory.GetLogger(typeof(PluginDiscoveryService));
    }

    /// <summary>
    /// Scans every immediate subdirectory of <paramref name="pluginsRootDirectory"/>
    /// for a <c>plugin.json</c> file, parsing each one found and flagging any
    /// bundled platform assembly copies as warnings.
    /// </summary>
    /// <param name="pluginsRootDirectory">The directory containing one subdirectory per plugin.</param>
    /// <param name="cancellationToken">Token used to cancel a long-running scan.</param>
    /// <returns>
    /// A descriptor for every subdirectory containing a manifest — successfully
    /// parsed ones in <see cref="PluginLoadState.NotLoaded"/>, and unparsable
    /// ones in <see cref="PluginLoadState.Failed"/> with a populated
    /// <see cref="IPluginDescriptor.FailureReason"/>.
    /// </returns>
    public Task<IReadOnlyList<IPluginDescriptor>> DiscoverAsync(string pluginsRootDirectory, CancellationToken cancellationToken)
    {
        var results = new List<IPluginDescriptor>();

        if (!Directory.Exists(pluginsRootDirectory))
        {
            _logger.Warn("Plugins root directory '{0}' does not exist; no plugins discovered.", pluginsRootDirectory);
            return Task.FromResult<IReadOnlyList<IPluginDescriptor>>(results);
        }

        foreach (var pluginDirectory in Directory.EnumerateDirectories(pluginsRootDirectory))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var manifestPath = Path.Combine(pluginDirectory, ManifestFileName);

            if (!File.Exists(manifestPath))
            {
                _logger.Debug("Skipping directory '{0}': no '{1}' found.", pluginDirectory, ManifestFileName);
                continue;
            }

            results.Add(TryParseDescriptor(pluginDirectory, manifestPath));
        }

        _logger.Info("Discovered {0} plugin(s) under '{1}'.", results.Count, pluginsRootDirectory);

        return Task.FromResult<IReadOnlyList<IPluginDescriptor>>(results);
    }

    /// <summary>
    /// Attempts to parse a single plugin's manifest and scan for redundant
    /// platform assembly copies, producing either a
    /// <see cref="PluginLoadState.NotLoaded"/> descriptor on success or a
    /// <see cref="PluginLoadState.Failed"/> descriptor with the failure reason on error.
    /// </summary>
    /// <param name="pluginDirectory">The plugin's directory.</param>
    /// <param name="manifestPath">The path to the plugin's manifest file.</param>
    /// <returns>The resulting descriptor, never throwing even if parsing failed.</returns>
    private IPluginDescriptor TryParseDescriptor(string pluginDirectory, string manifestPath)
    {
        try
        {
            var manifest = _manifestParser.Parse(manifestPath);
            var descriptor = new PluginDescriptorImpl(manifest, pluginDirectory, PluginLoadState.NotLoaded, failureReason: null);

            var warnings = _redundantAssemblyDetector.Scan(pluginDirectory);
            if (warnings.Count > 0)
            {
                descriptor.SetWarnings(warnings);
                foreach (var warning in warnings)
                {
                    _logger.Warn("Plugin '{0}': {1}", manifest.Id, warning);
                }
            }

            return descriptor;
        }
        catch (Diagnostics.PluginManifestParseException ex)
        {
            _logger.Error("Failed to parse manifest for plugin directory '{0}'", ex, pluginDirectory);
            return PluginDescriptorImpl.CreateFailed(pluginDirectory, ex.Message);
        }
    }
}