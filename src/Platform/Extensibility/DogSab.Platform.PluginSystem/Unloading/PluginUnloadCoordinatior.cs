using System.Runtime.Loader;
using DogSab.Platform.Core.Abstractions.Logging;
using DogSab.Platform.Extensibility.Abstractions.Loading;
using DogSab.Platform.Extensibility.Abstractions.Manifest;
using DogSab.Platform.PluginSystem.Loading;

namespace DogSab.Platform.PluginSystem.Unloading;

/// <summary>
/// Verifies that a plugin's <see cref="AssemblyLoadContext"/> has actually
/// been collected after <see cref="IPluginLoader.Unload"/> requests it,
/// rather than assuming the request succeeded. Calling <c>Unload()</c> on a
/// collectible context only marks it eligible for collection; the CLR only
/// reclaims it once no live references remain into any of its loaded
/// assemblies. If the platform (or another plugin) still holds so much as one
/// reference to an object whose type came from the unloaded plugin, the
/// context is retained forever — silently, with no exception — which is
/// exactly the kind of leak this coordinator exists to detect and report.
/// </summary>
public sealed class PluginUnloadCoordinator
{
    /// <summary>How many GC passes to attempt before giving up and reporting the context as stuck.</summary>
    private const int MaxCollectionAttempts = 10;

    /// <summary>Delay between collection attempts, giving finalizers and pending async work a chance to complete.</summary>
    private static readonly TimeSpan CollectionRetryDelay = TimeSpan.FromMilliseconds(100);

    /// <summary>Logger used to report unload progress and failures.</summary>
    private readonly ILogger _logger;

    /// <summary>
    /// Creates a new unload coordinator.
    /// </summary>
    /// <param name="loggerFactory">Factory used to obtain a logger scoped to this coordinator.</param>
    public PluginUnloadCoordinator(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.GetLogger(typeof(PluginUnloadCoordinator));
    }

    /// <summary>
    /// Requests unload of a plugin's load context and repeatedly forces
    /// garbage collection, waiting for the context to actually become
    /// collected. Returns whether it was confirmed unloaded within the
    /// attempted window — a <c>false</c> result does not mean unloading
    /// failed permanently, only that it did not complete promptly, which
    /// usually indicates a lingering reference somewhere in the platform or
    /// another plugin.
    /// </summary>
    /// <param name="pluginId">The plugin whose context is being unloaded, for logging.</param>
    /// <param name="loadContext">The load context to unload. The caller must not use this reference again after calling this method.</param>
    /// <param name="cancellationToken">Token used to abort the verification wait early.</param>
    /// <returns>A task producing <c>true</c> if the context was confirmed collected; <c>false</c> if it appears stuck after all attempts.</returns>
    public async Task<bool> UnloadAndVerifyAsync(
        PluginId pluginId,
        PluginAssemblyLoadContext loadContext,
        CancellationToken cancellationToken)
    {
        // A WeakReference is essential here: holding a strong reference to
        // loadContext for the rest of this method would itself prevent
        // collection, defeating the entire point of the check.
        var weakContextReference = new WeakReference(loadContext, trackResurrection: true);

        loadContext.Unload();
        loadContext = null!; // drop the last strong local reference explicitly

        _logger.Debug("Unload requested for plugin '{0}'; verifying collection.", pluginId);

        for (var attempt = 1; attempt <= MaxCollectionAttempts; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            if (!weakContextReference.IsAlive)
            {
                _logger.Info("Plugin '{0}' load context collected after {1} attempt(s).", pluginId, attempt);
                return true;
            }

            await Task.Delay(CollectionRetryDelay, cancellationToken).ConfigureAwait(false);
        }

        _logger.Warn(
            "Plugin '{0}' load context was NOT collected after {1} attempts. " +
            "Something outside the plugin still holds a live reference into it — " +
            "the plugin's memory will remain resident until that reference is released.",
            pluginId,
            MaxCollectionAttempts);

        return false;
    }
}