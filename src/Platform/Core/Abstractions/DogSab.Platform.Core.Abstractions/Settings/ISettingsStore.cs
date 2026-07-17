namespace DogSab.Platform.Core.Abstractions.Settings;

/// <summary>Persists and loads plain settings objects to/from disk.</summary>
public interface ISettingsStore
{
    /// <summary>
    /// Loads a settings instance of the given type, creating a default one if none is stored yet.
    /// </summary>
    /// <typeparam name="T">The settings type to load.</typeparam>
    /// <returns>The loaded (or newly created) settings instance.</returns>
    T Load<T>() where T : class, new();

    /// <summary>
    /// Persists a settings instance to disk.
    /// </summary>
    /// <typeparam name="T">The settings type to save.</typeparam>
    /// <param name="instance">The instance whose state should be persisted.</param>
    void Save<T>(T instance) where T : class;
}