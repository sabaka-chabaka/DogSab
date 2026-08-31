
using System.Collections.Concurrent;
using DogSab.Platform.RunConfigurations.Abstractions;

namespace DogSab.Platform.RunConfigurations;

/// <summary>
/// Holds every user-created <see cref="IRunConfiguration"/> for the current
/// session, keyed by <see cref="RunConfigurationId"/>.
/// Distinct from <see cref="Extensibility.Registry.ExtensionPointRegistryImpl"/>-style
/// registries elsewhere in the platform — those hold plugin-contributed
/// extension implementations, discovered once at load time; this holds
/// user-authored data (a specific person's specific saved run setups),
/// added and removed interactively rather than declared by a plugin
/// manifest.
/// </summary>
public sealed class RunConfigurationManagerImpl
{
    /// <summary>
    /// Every currently known configuration, keyed by its ID.
    /// </summary>
    private readonly ConcurrentDictionary<RunConfigurationId, IRunConfiguration> _configurationsById = new();

    /// <summary>
    /// Raised whenever a configuration is added or removed, so a future
    /// run configuration dropdown UI can refresh its list.
    /// </summary>
    public event Action? ConfigurationsChanged;

    /// <summary>
    /// Adds a new run configuration, or replaces an existing one with the
    /// same ID.
    /// </summary>
    /// <param name="configuration">
    /// The configuration to add.
    /// </param>
    public void Add(IRunConfiguration configuration)
    {
        _configurationsById[configuration.Id] = configuration;
        ConfigurationsChanged?.Invoke();
    }

    /// <summary>
    /// Removes a configuration by its ID, if present.
    /// </summary>
    /// <param name="id">
    /// The identifier of the configuration to remove.
    /// </param>
    public void Remove(RunConfigurationId id)
    {
        if (_configurationsById.TryRemove(id, out _))
        {
            ConfigurationsChanged?.Invoke();
        }
    }

    /// <summary>
    /// Resolves a configuration by its ID.
    /// </summary>
    /// <param name="id">
    /// The identifier of the configuration to look up.
    /// </param>
    /// <returns>
    /// The configuration, or <c>null</c> if no configuration with this ID exists.
    /// </returns>
    public IRunConfiguration? Find(RunConfigurationId id)
    {
        return _configurationsById.TryGetValue(id, out var configuration) ? configuration : null;
    }

    /// <summary>
    /// Every currently configured run configuration, in no particular
    /// guaranteed order.
    /// </summary>
    public IReadOnlyList<IRunConfiguration> AllConfigurations => _configurationsById.Values.ToList();
}