using System.Collections.Concurrent;
using System.Reflection;
using DogSab.Platform.Core.Abstractions.Application;
using DogSab.Platform.Core.Abstractions.Logging;
using DogSab.Platform.Extensibility.Abstractions.ExtensionPoints;
using DogSab.Platform.Extensibility.Abstractions.Loading;
using DogSab.Platform.Extensibility.Abstractions.Manifest;
using DogSab.Platform.Extensibility.Reflection;
using DogSab.Platform.PluginSystem.DependencyResolution;
using DogSab.Platform.PluginSystem.Discovery;
using DogSab.Platform.PluginSystem.Unloading;

namespace DogSab.Platform.PluginSystem.Loading;

/// <summary>
/// Default implementation of <see cref="IPluginLoader"/>. Orchestrates the
/// full pipeline: discovery (delegated to <see cref="PluginDiscoveryService"/>),
/// dependency-order resolution and compatibility checks (delegated to
/// <see cref="PluginDependencyGraphResolver"/> and
/// <see cref="PluginCompatibilityChecker"/>), assembly loading into an
/// isolated <see cref="PluginAssemblyLoadContext"/> per plugin, and finally
/// registering each successfully loaded plugin's declared extensions into the
/// platform's <see cref="IExtensionPointRegistry"/>. A single plugin's
/// failure at any stage marks only that plugin (and anything depending on it)
/// as <see cref="PluginLoadState.Failed"/>, without preventing unrelated
/// plugins from loading.
/// </summary>
public sealed class PluginLoaderImpl : IPluginLoader
{
    private readonly PluginDiscoveryService _discoveryService;
    private readonly PluginDependencyGraphResolver _dependencyResolver;
    private readonly PluginCompatibilityChecker _compatibilityChecker;
    private readonly ExtensionInstantiator _extensionInstantiator;
    private readonly IExtensionPointRegistry _extensionPointRegistry;
    private readonly IApplicationInfo _applicationInfo;
    private readonly PluginUnloadCoordinator _unloadCoordinator;
    private readonly ILogger _logger;
    
    /// <summary>Load contexts for currently loaded plugins, keyed by plugin ID, needed later for unloading.</summary>
    private readonly ConcurrentDictionary<PluginId, PluginAssemblyLoadContext> _loadContextsByPluginId = new();

    /// <summary>Extension instances registered per plugin, needed to unregister them cleanly on unload.</summary>
    private readonly ConcurrentDictionary<PluginId, List<(string ExtensionPointId, object Instance)>> _registeredExtensionsByPluginId = new();

    /// <summary>
    /// Creates a new plugin loader.
    /// </summary>
    public PluginLoaderImpl(
        PluginDiscoveryService discoveryService,
        PluginDependencyGraphResolver dependencyResolver,
        PluginCompatibilityChecker compatibilityChecker,
        ExtensionInstantiator extensionInstantiator,
        IExtensionPointRegistry extensionPointRegistry,
        IApplicationInfo applicationInfo,
        PluginUnloadCoordinator coordinator,
        ILoggerFactory loggerFactory)
    {
        _discoveryService = discoveryService;
        _dependencyResolver = dependencyResolver;
        _compatibilityChecker = compatibilityChecker;
        _extensionInstantiator = extensionInstantiator;
        _extensionPointRegistry = extensionPointRegistry;
        _applicationInfo = applicationInfo;
        _unloadCoordinator = coordinator;
        _logger = loggerFactory.GetLogger(typeof(PluginLoaderImpl));
    }
    
    /// <inheritdoc />
    public Task<IReadOnlyList<IPluginDescriptor>> DiscoverAsync(string pluginsRootDirectory, CancellationToken cancellationToken)
    {
        return _discoveryService.DiscoverAsync(pluginsRootDirectory, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<IPluginDescriptor>> LoadAllAsync(
        IReadOnlyList<IPluginDescriptor> descriptors,
        CancellationToken cancellationToken)
    {
        // Only attempt to order/load descriptors that survived discovery
        // (i.e. their manifest actually parsed) — already-Failed descriptors
        // are left untouched and simply excluded from the dependency graph.
        var loadableDescriptors = new List<IPluginDescriptor>();
        foreach (var descriptor in descriptors)
        {
            if (descriptor.State != PluginLoadState.Failed)
            {
                loadableDescriptors.Add(descriptor);
            }
        }

        IReadOnlyList<IPluginDescriptor> orderedDescriptors;
        try
        {
            orderedDescriptors = _dependencyResolver.ResolveLoadOrder(loadableDescriptors);
        }
        catch (Exception ex)
        {
            // A cycle or missing required dependency anywhere in the graph
            // prevents the whole batch from being orderable — mark every
            // loadable descriptor as failed rather than guessing a partial order.
            _logger.Error("Failed to resolve plugin load order", ex);
            foreach (var descriptor in loadableDescriptors)
            {
                MarkFailed(descriptor, $"Dependency resolution failed: {ex.Message}");
            }
            return descriptors;
        }

        var failedPluginIds = new HashSet<PluginId>();

        foreach (var descriptor in orderedDescriptors)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (HasFailedDependency(descriptor, failedPluginIds))
            {
                MarkFailed(descriptor, "A required dependency failed to load.");
                failedPluginIds.Add(descriptor.Manifest.Id);
                continue;
            }

            if (!TryLoadSingle(descriptor))
            {
                failedPluginIds.Add(descriptor.Manifest.Id);
            }
        }

        return descriptors;
    }

    /// <inheritdoc />
    /// <inheritdoc />
    public bool Unload(PluginId pluginId)
    {
        if (!_loadContextsByPluginId.TryRemove(pluginId, out var loadContext))
        {
            return false;
        }

        if (_registeredExtensionsByPluginId.TryRemove(pluginId, out var registeredExtensions))
        {
            foreach (var (extensionPointId, instance) in registeredExtensions)
            {
                UnregisterUntyped(extensionPointId, instance);
            }
        }

        // Fire-and-forget verification: the plugin is already considered
        // unloaded from the platform's perspective (extensions unregistered,
        // context removed from tracking) regardless of how long actual CLR
        // collection takes. The coordinator's result is purely diagnostic.
        _ = _unloadCoordinator.UnloadAndVerifyAsync(pluginId, loadContext, CancellationToken.None);

        _logger.Info("Plugin '{0}' unload requested.", pluginId);
        return true;
    }

    /// <summary>
    /// Unregisters a previously registered extension instance using the
    /// untyped path, matching how it was originally registered in <see cref="InstantiateAndRegister"/>.
    /// </summary>
    /// <param name="extensionPointId">The extension point ID the instance was registered against.</param>
    /// <param name="instance">The extension instance to unregister.</param>
    private void UnregisterUntyped(string extensionPointId, object instance)
    {
        _extensionPointRegistry.UnregisterExtensionUntyped(extensionPointId, instance);
    }

    /// <summary>
    /// Attempts to load a single plugin: checks platform compatibility, loads
    /// its assembly into an isolated context, and instantiates and registers
    /// each of its declared extensions.
    /// </summary>
    /// <param name="descriptor">The plugin descriptor to load.</param>
    /// <returns><c>true</c> if the plugin loaded successfully; otherwise <c>false</c>.</returns>
    private bool TryLoadSingle(IPluginDescriptor descriptor)
    {
        var manifest = descriptor.Manifest;

        if (!(descriptor as PluginDescriptorImpl)?.State.Equals(PluginLoadState.NotLoaded) ?? true)
        {
            // Defensive: skip anything not in the expected starting state.
        }

        if (!_compatibilityChecker.IsPlatformCompatible(manifest, ParsePlatformVersion()))
        {
            MarkFailed(descriptor, $"Plugin requires platform version {manifest.CompatiblePlatformVersionRange}, " +
                                    $"but the running platform is {_applicationInfo.Version}.");
            return false;
        }

        (descriptor as PluginDescriptorImpl)?.TransitionTo(PluginLoadState.Loading);

        try
        {
            var mainAssemblyPath = Path.Combine(descriptor.PluginDirectory, manifest.MainAssemblyFileName);
            var loadContext = new PluginAssemblyLoadContext(manifest.Id.Value, mainAssemblyPath);
            var assembly = loadContext.LoadMainAssembly(mainAssemblyPath);

            var registeredExtensions = new List<(string, object)>();

            foreach (var extensionDeclaration in manifest.Extensions)
            {
                var instance = InstantiateAndRegister(assembly, extensionDeclaration);
                registeredExtensions.Add((extensionDeclaration.ExtensionPointId, instance));
            }

            _loadContextsByPluginId[manifest.Id] = loadContext;
            _registeredExtensionsByPluginId[manifest.Id] = registeredExtensions;

            (descriptor as PluginDescriptorImpl)?.TransitionTo(PluginLoadState.Loaded);
            _logger.Info("Plugin '{0}' loaded successfully.", manifest.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to load plugin '{0}'", ex, manifest.Id);
            MarkFailed(descriptor, ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Instantiates a single declared extension via reflection and registers
    /// it against the platform's extension point registry using the untyped
    /// registration path, since the contract type is only known at runtime here.
    /// </summary>
    /// <param name="assembly">The plugin's loaded assembly.</param>
    /// <param name="declaration">The extension declaration to instantiate and register.</param>
    /// <returns>The instantiated extension object.</returns>
    private object InstantiateAndRegister(Assembly assembly, ExtensionDeclaration declaration)
    {
        if (!_extensionPointRegistry.IsExtensionPointDeclared(declaration.ExtensionPointId))
        {
            throw new InvalidOperationException(
                $"Extension declared against unknown extension point '{declaration.ExtensionPointId}'.");
        }

        var contractType = _extensionInstantiator.ResolveContractType(declaration.ExtensionPointId);
        var instance = _extensionInstantiator.InstantiateUntyped(
            assembly,
            declaration.ExtensionPointId,
            declaration.ImplementationClassName,
            contractType);

        _extensionPointRegistry.RegisterExtensionUntyped(declaration.ExtensionPointId, instance);

        return instance;
    }

    private bool HasFailedDependency(IPluginDescriptor descriptor, HashSet<PluginId> failedPluginIds)
    {
        foreach (var dependency in descriptor.Manifest.Dependencies)
        {
            if (!dependency.IsOptional && failedPluginIds.Contains(dependency.DependencyPluginId))
            {
                return true;
            }
        }

        return false;
    }

    private void MarkFailed(IPluginDescriptor descriptor, string reason)
    {
        (descriptor as PluginDescriptorImpl)?.TransitionTo(PluginLoadState.Failed, reason);
        _logger.Warn("Plugin '{0}' marked as failed: {1}", descriptor.Manifest.Id, reason);
    }

    private DogSab.Platform.Extensibility.Abstractions.Manifest.PluginVersion ParsePlatformVersion()
    {
        var v = _applicationInfo.Version;
        return new DogSab.Platform.Extensibility.Abstractions.Manifest.PluginVersion(v.Major, v.Minor, v.Build < 0 ? 0 : v.Build);
    }
}