using System.IO.Compression;
using DogSab.Platform.Core.Abstractions.Logging;

namespace DogSab.Platform.PluginManager.Installation;

/// <summary>
/// Extracts a plugin's <c>.zip</c> archive into the platform's plugins
/// directory, as a subdirectory named after the archive itself (with its
/// extension stripped). Purely a filesystem operation — this does not
/// validate that the extracted contents form a valid plugin (that a
/// <c>plugin.json</c> exists and parses); that check happens afterward in
/// <see cref="PluginInstaller"/>, which uses the platform's existing
/// <c>PluginSystem.Manifest.PluginManifestParser</c> rather than
/// duplicating manifest validation here.
/// </summary>
public sealed class PluginArchiveExtractor
{
    /// <summary>
    /// Logger used to report extraction failures.
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// Creates a new plugin archive extractor.
    /// </summary>
    /// <param name="loggerFactory">
    /// Factory used to obtain a logger scoped to this extractor.
    /// </param>
    public PluginArchiveExtractor(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.GetLogger(typeof(PluginArchiveExtractor));
    }

    /// <summary>
    /// Extracts a plugin archive into a new subdirectory of the given
    /// plugins root directory.
    /// </summary>
    /// <param name="archiveFilePath">
    /// The path to the <c>.zip</c> archive to extract.
    /// </param>
    /// <param name="pluginsRootDirectory">
    /// The platform's plugins directory to extract into.
    /// </param>
    /// <returns>
    /// The path to the newly created plugin subdirectory.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if a directory with the derived name already exists in
    /// <paramref name="pluginsRootDirectory"/>, or if extraction otherwise fails.
    /// </exception>
    public string Extract(string archiveFilePath, string pluginsRootDirectory)
    {
        var pluginDirectoryName = Path.GetFileNameWithoutExtension(archiveFilePath);
        var targetDirectory = Path.Combine(pluginsRootDirectory, pluginDirectoryName);

        if (Directory.Exists(targetDirectory))
        {
            throw new InvalidOperationException(
                $"A plugin directory named '{pluginDirectoryName}' already exists. " +
                $"Uninstall the existing plugin first, or rename the archive before installing.");
        }

        try
        {
            Directory.CreateDirectory(targetDirectory);
            ZipFile.ExtractToDirectory(archiveFilePath, targetDirectory);
            
            _logger.Info("Extracted plugin archive '{0}' to '{1}'", archiveFilePath, targetDirectory);
            
            return targetDirectory;
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to extract plugin archive '{0}'", ex, archiveFilePath);

            if (Directory.Exists(targetDirectory))
            {
                Directory.Delete(targetDirectory, true);
            }

            throw new InvalidOperationException($"Failed to extract plugin archive '{archiveFilePath}'.", ex);
        }
    }
}