namespace DogSab.Platform.Extensibility.Abstractions.Loading;

/// <summary>
/// The current position of a discovered plugin in its loading lifecycle,
/// surfaced to diagnostics and the Plugin Manager UI so a failed or disabled
/// plugin's state is visible rather than sliently absent from the running system.
/// </summary>
public enum PluginLoadState
{
    /// <summary>The manifest has been discovered and parsed, but the plugin's assembly has not been loaded yet.</summary>
    NotLoaded,

    /// <summary>The plugin's assembly is currently being loaded and its extensions registered.</summary>
    Loading,

    /// <summary>The plugin loaded successfully and all its declared extensions were registered.</summary>
    Loaded,

    /// <summary>
    /// Loading failed — due to a missing/incompatible required dependency,
    /// a missing main assembly, a reflection failure locating a declared
    /// extension class, or an exception during extension instantiation.
    /// See the associated <see cref="IPluginDescriptor.FailureReason"/> for details.
    /// </summary>
    Failed,

    /// <summary>The plugin was explicitly disabled by the user and is skipped during loading, even if otherwise valid.</summary>
    Disabled
}