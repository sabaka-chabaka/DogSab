using DogSab.Platform.Core.Abstractions.Logging;
using DogSab.Platform.Extensibility.Abstractions.Loading;
using DogSab.Platform.Extensibility.Abstractions.Manifest;

namespace DogSab.Platform.PluginManager.Installation;

/// <summary>
/// Removes an installed plugin: unloads it from the running process first
/// if currently loaded (via the platform's existing
/// <see cref="IPluginLoader.Unload"/>), then deletes its directory from disk.
/// Unloading before deletion matters specifically because of how
/// <c>PluginAssemblyLoadContext</c> works — deleting a plugin's assembly
/// files out from under a still-loaded, collectible load context risks the
/// CLR failing to properly finalize collection of that context, since its
/// assemblies' backing files would no longer exist for whatever internal
/// bookkeeping the runtime performs during unload.
/// </summary>
public sealed class PluginUninstaller
{
    /// <summary>
    /// The platform's plugin loader, used to unload a plugin before its
    /// files are deleted.
    /// </summary>
    private readonly IPluginLoader _pluginLoader;

    /// <summary>
    /// Logger used to report uninstallation progress and failures.
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// Creates a new plugin uninstaller.
    /// </summary>
    /// <param name="pluginLoader">
    /// The platform's plugin loader, used to unload a plugin before removal.
    /// </param>
    /// <param name="loggerFactory">
    /// Factory used to obtain a logger scoped to this uninstaller.
    /// </param>
    public PluginUninstaller(IPluginLoader pluginLoader, ILoggerFactory loggerFactory)
    {
        _pluginLoader = pluginLoader;
        _logger = loggerFactory.GetLogger(typeof(PluginUninstaller));
    }

    /// <summary>
    /// Uninstalls a plugin: unloads it if currently loaded, then deletes
    /// its directory from disk.
    /// </summary>
    /// <param name="pluginId">
    /// The identifier of the plugin to uninstall.
    /// </param>
    /// <param name="pluginDirectory">
    /// The plugin's installation directory to delete.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if deleting the plugin's directory fails, e.g. because a
    /// file within it is still locked despite the unload attempt — this
    /// can genuinely happen if unloading did not complete promptly (see
    /// the known limitation on <see cref="PluginSystem.Unloading.PluginUnloadCoordinator"/>
    /// regarding lingering references delaying actual CLR collection).
    /// </exception>
    public void Uninstall(PluginId pluginId, string pluginDirectory)
    {
        _pluginLoader.Unload(pluginId);

        try
        {
            Directory.Delete(pluginDirectory, true);
            _logger.Info("Uninstalled plugin '{0}' from '{1}'.", pluginId, pluginDirectory);
        }
        catch (IOException ex)
        {
            _logger.Error(
                "Failed to delete plugin directory '{0}' for plugin '{1}' — " +
                "its assembly files may still be locked if unloading has not fully completed yet.",
                ex,
                pluginDirectory,
                pluginId);

            throw new InvalidOperationException(
                $"Could not delete plugin directory for '{pluginId}'. " +
                $"The plugin may need a moment to fully unload, or the application may need to be restarted.",
                ex);
        }
    }
}
