using DogSab.Platform.Core.Abstractions.Logging;
using DogSab.Platform.Extensibility.Abstractions.Loading;
using DogSab.Platform.PluginSystem.Diagnostics;
using DogSab.Platform.PluginSystem.Manifest;

namespace DogSab.Platform.PluginManager.Installation;

/// <summary>
/// Installs a plugin from a local <c>.zip</c> archive: extracts it into the
/// plugins directory via <see cref="PluginArchiveExtractor"/>, then
/// validates the extracted contents actually form a loadable plugin by
/// parsing its manifest with the platform's existing
/// <see cref="PluginManifestParser"/> — the same parser <c>PluginSystem</c>
/// itself uses during normal discovery, so a plugin installed this way is
/// held to exactly the same validity standard as one dropped into the
/// plugins folder manually before startup.
/// If validation fails, the partially-installed directory is removed
/// rather than left behind as a broken entry future discovery passes would
/// stumble over.
/// </summary>
public sealed class PluginInstaller
{
    /// <summary>
    /// Extracts the archive into the plugins directory.
    /// </summary>
    private readonly PluginArchiveExtractor _extractor;

    /// <summary>
    /// Parses and validates the extracted plugin's manifest, reusing the
    /// same parser <see cref="PluginSystem.Discovery.PluginDiscoveryService"/>
    /// uses during normal startup discovery.
    /// </summary>
    private readonly PluginManifestParser _manifestParser;

    /// <summary>
    /// Logger used to report installation failures.
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// Creates a new plugin installer.
    /// </summary>
    /// <param name="extractor">
    /// The extractor used to unpack the archive.
    /// </param>
    /// <param name="manifestParser">
    /// The parser used to validate the extracted plugin's manifest.
    /// </param>
    /// <param name="loggerFactory">
    /// Factory used to obtain a logger scoped to this installer.
    /// </param>
    public PluginInstaller(PluginArchiveExtractor extractor, PluginManifestParser manifestParser,
        ILoggerFactory loggerFactory)
    {
        _extractor = extractor;
        _manifestParser = manifestParser;
        _logger = loggerFactory.GetLogger(typeof(PluginInstaller));
    }

    /// <summary>
    /// The conventional file name for a plugin's manifest within its
    /// directory, matching the constant used by
    /// <see cref="PluginSystem.Discovery.PluginDiscoveryService"/>.
    /// </summary>
    private const string ManifestFileName = "plugin.json";

    /// <summary>
    /// Installs a plugin from a local archive file.
    /// </summary>
    /// <param name="archiveFilePath">
    /// The path to the <c>.zip</c> archive to install.
    /// </param>
    /// <param name="pluginsRootDirectory">
    /// The platform's plugins directory to install into.
    /// </param>
    /// <returns>
    /// A descriptor for the newly installed plugin, in
    /// <see cref="PluginLoadState.NotLoaded"/> — installing does not itself
    /// load the plugin into the running process; that still requires a
    /// separate call to <see cref="IPluginLoader.LoadAllAsync"/>, or a
    /// restart, matching how newly discovered plugins are always handled
    /// by the platform.
    /// </returns>
    /// <exception cref="PluginManifestParseException">
    /// Thrown if the extracted archive does not contain a valid
    /// <c>plugin.json</c>, after removing the invalid extracted directory.
    /// </exception>
    public IPluginDescriptor Install(string archiveFilePath, string pluginsRootDirectory)
    {
        var extractedDirectory = _extractor.Extract(archiveFilePath, pluginsRootDirectory);

        try
        {
            var manifestPath = Path.Combine(extractedDirectory, ManifestFileName);
            var manifest = _manifestParser.Parse(manifestPath);
            
            _logger.Info("Installed plugin '{0}' (version {1}) to '{2}'.", manifest.Id, manifest.Version, extractedDirectory);

            return new PluginSystem.Loading.PluginDescriptorImpl(manifest, extractedDirectory, PluginLoadState.NotLoaded, failureReason: null);
        }
        catch (PluginManifestParseException ex)
        {
            _logger.Error("Installed archive '{0}' does not contain a valid plugin; removing extracted directory.", ex, archiveFilePath);
            
            Directory.Delete(extractedDirectory, true);
            
            throw;
        }
    }
}