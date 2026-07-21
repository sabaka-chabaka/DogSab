using DogSab.Platform.Core.Abstractions.Logging;
using DogSab.Platform.Core.Abstractions.Settings;
using DogSab.Platform.Core.Settings.Impl.Paths;
using DogSab.Platform.Core.Settings.Impl.Serialization;

namespace DogSab.Platform.Core.Settings.Impl.Store;

/// <summary>
/// Default implementation of <see cref="ISettingsStore"/>.
/// Reads the target settings type's <see cref="PersistentStateAttribute"/> to
/// determine the file name and scope, resolves the on-disk path via
/// <see cref="SettingsPathResolver"/>, and (de)serializes via <see cref="XmlSettingsSerializer"/>.
/// If no file exists yet for a requested type, <see cref="Load{T}"/> returns a
/// fresh default instance rather than throwing, matching first-run behavior.
/// </summary>
public sealed class SettingsStoreImpl : ISettingsStore
{
    /// <summary>Resolves on-disk paths per settings scope.</summary>
    private readonly SettingsPathResolver _pathResolver;

    /// <summary>Performs the actual XML (de)serialization.</summary>
    private readonly XmlSettingsSerializer _serializer = new();

    /// <summary>Logger used to report load/save failures.</summary>
    private readonly ILogger _logger;

    /// <summary>
    /// Creates a new settings store.
    /// </summary>
    /// <param name="pathResolver">Resolver used to determine on-disk settings file paths.</param>
    /// <param name="loggerFactory">Factory used to obtain a logger scoped to this store.</param>
    public SettingsStoreImpl(SettingsPathResolver pathResolver, ILoggerFactory loggerFactory)
    {
        _pathResolver = pathResolver;
        _logger = loggerFactory.GetLogger(typeof(SettingsStoreImpl));
    }
    
    /// <summary>
    /// Loads a settings instance of the given type, creating a default one if none is stored yet or if the stored file could not be read.
    /// </summary>
    /// <typeparam name="T">The settings type to load.</typeparam>
    /// <returns>The loaded, or newly created default, settings instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown if <typeparamref name="T"/> is not marked with <see cref="PersistentStateAttribute"/>.</exception>
    public T Load<T>() where T : class, new()
    {
        var attribute = GetPersistentStateAttribute<T>();
        var filePath = _pathResolver.ResolveFilePath(attribute.FileName, attribute.Scope);

        if (!File.Exists(filePath))
        {
            return new T();
        }

        try
        {
            return _serializer.Deserialize<T>(filePath);
        }
        catch (Exception ex)
        {
            _logger.Error(
                "Failed to load settings of type '{0}' from '{1}'; failing back to default",
                ex,
                typeof(T).FullName ?? typeof(T).Name,
                filePath);
            return new T();
        }
    }
    
    /// <summary>
    /// Persists a settings instance to disk, at the path and scope declared by
    /// its <see cref="PersistentStateAttribute"/>.
    /// </summary>
    /// <typeparam name="T">The settings type to save.</typeparam>
    /// <param name="instance">The instance whose state should be persisted.</param>
    /// <exception cref="InvalidOperationException">Thrown if <typeparamref name="T"/> is not marked with <see cref="PersistentStateAttribute"/>.</exception>
    public void Save<T>(T instance) where T : class
    {
        var attribute = GetPersistentStateAttribute<T>();
        var filePath = _pathResolver.ResolveFilePath(attribute.FileName, attribute.Scope);

        try
        {
            _serializer.Serialize(filePath, instance);
        }
        catch (Exception ex)
        {
            _logger.Error(
                "Failed to save settings of type '{0}' to '{1}'",
                ex,
                typeof(T).FullName ?? typeof(T).Name,
                filePath);

            throw;
        }
    }
    
    /// <summary>
    /// Reads the required <see cref="PersistentStateAttribute"/> from a settings type.
    /// </summary>
    /// <typeparam name="T">The settings type to inspect.</typeparam>
    /// <returns>The type's persistent-state attribute.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the type is not annotated with the attribute.</exception>
    private static PersistentStateAttribute GetPersistentStateAttribute<T>()
    {
        var attribute = (PersistentStateAttribute?)Attribute.GetCustomAttribute(typeof(T), typeof(PersistentStateAttribute));

        return attribute ?? throw new InvalidOperationException(
            $"Type '{typeof(T).FullName}' must be annotated with [{nameof(PersistentStateAttribute)}] to be used with {nameof(ISettingsStore)}.");
    }
}