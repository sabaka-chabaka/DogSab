using DogSab.Platform.Extensibility.Abstractions.Manifest;

namespace DogSab.Platform.Extensibility.Abstractions.Loading;

/// <summary>
/// Contract for discovering, validating, and loading plugins from disk.
/// The concrete implementation (in <c>DogSab.Platform.PluginSystem</c>) is
/// responsible for manifest parsing, dependency-order resolution, creating an
/// isolated <c>AssemblyLoadContext</c> per plugin, and registering each
/// plugin's declared extensions into the platform's
/// <see cref="ExtensionPoints.IExtensionPointRegistry"/>.
/// </summary>
public interface IPluginLoader
{
    /// <summary>
    /// Scans the given directory for plugin subdirectories containing a valid
    /// manifest, without loading any plugin assemblies yet. Used to populate
    /// the Plugin Manager UI's list before the user chooses to enable/disable
    /// individual plugins.
    /// </summary>
    /// <param name="pluginsRootDirectory">The directory containing one subdirectory per plugin.</param>
    /// <param name="cancellationToken">Token used to cancel a long-running scan.</param>
    /// <returns>A descriptor for every plugin found, each with <see cref="IPluginDescriptor.State"/> equal to <see cref="PluginLoadState.NotLoaded"/>.</returns>
    Task<IReadOnlyList<IPluginDescriptor>> DiscoverAsync(string pluginsRootDirectory, CancellationToken cancellationToken);

    /// <summary>
    /// Loads every non-disabled plugin from a prior <see cref="DiscoverAsync"/>
    /// call, in dependency order, registering each successfully loaded plugin's
    /// extensions into the platform's extension point registry. Plugins whose
    /// required dependencies are missing or incompatible are marked
    /// <see cref="PluginLoadState.Failed"/> and skipped, without preventing
    /// unrelated plugins from loading.
    /// </summary>
    /// <param name="descriptors">The plugin descriptors to load, as returned by <see cref="DiscoverAsync"/>.</param>
    /// <param name="cancellationToken">Token used to cancel a long-running load sequence.</param>
    /// <returns>
    /// The same descriptors, with <see cref="IPluginDescriptor.State"/> updated
    /// to reflect the outcome of loading each one.
    /// </returns>
    Task<IReadOnlyList<IPluginDescriptor>> LoadAllAsync(IReadOnlyList<IPluginDescriptor> descriptors, CancellationToken cancellationToken);

    /// <summary>
    /// Unloads a previously loaded plugin: unregisters its extensions from the
    /// registry and requests collection of its isolated load context. Used when
    /// the user disables a plugin or installs an updated version without
    /// restarting the application.
    /// </summary>
    /// <param name="pluginId">The identifier of the plugin to unload.</param>
    /// <returns><c>true</c> if the plugin was found and unloaded; otherwise <c>false</c>.</returns>
    bool Unload(PluginId pluginId);
}