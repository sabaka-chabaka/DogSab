using DogSab.Platform.Extensibility.Abstractions.Loading;
using DogSab.Platform.Extensibility.Abstractions.Manifest;
using DogSab.Platform.PluginSystem.Manifest;

namespace DogSab.Platform.PluginSystem.Loading;

/// <summary>
/// Default implementation of <see cref="IPluginDescriptor"/>.
/// Unlike most value-holding types in the platform, this one is intentionally
/// mutable in its <see cref="State"/> and <see cref="FailureReason"/>: a
/// single descriptor instance is created once during discovery and then
/// updated in place as the same plugin progresses through
/// <see cref="PluginLoadState.NotLoaded"/> → <see cref="PluginLoadState.Loading"/>
/// → <see cref="PluginLoadState.Loaded"/>/<see cref="PluginLoadState.Failed"/>,
/// so the Plugin Manager UI can observe a stable object reference across the
/// whole lifecycle rather than being handed a new descriptor at each stage.
/// </summary>
public sealed class PluginDescriptorImpl : IPluginDescriptor
{
    /// <inheritdoc />
    public IPluginManifest Manifest { get; }

    /// <inheritdoc />
    public string PluginDirectory { get; }

    /// <inheritdoc />
    public PluginLoadState State { get; private set; }

    /// <inheritdoc />
    public string? FailureReason { get; private set; }
    
    /// <summary>
    /// Non-fatal warnings discovered about this plugin, such as bundling a
    /// redundant copy of a platform assembly. Does not affect
    /// <see cref="State"/> or prevent loading — purely informational, surfaced
    /// in the Plugin Manager UI to help plugin authors clean up their build output.
    /// </summary>
    public IReadOnlyList<string> Warnings { get; private set; } = System.Array.Empty<string>();
    
    /// <summary>
    /// Creates a new plugin descriptor for a plugin whose manifest parsed successfully.
    /// </summary>
    /// <param name="manifest">The plugin's parsed manifest.</param>
    /// <param name="pluginDirectory">The absolute path to the plugin's directory.</param>
    /// <param name="initialState">The descriptor's initial lifecycle state.</param>
    /// <param name="failureReason">An initial failure reason, if <paramref name="initialState"/> is already <see cref="PluginLoadState.Failed"/>.</param>
    public PluginDescriptorImpl(IPluginManifest manifest, string pluginDirectory, PluginLoadState initialState, string? failureReason)
    {
        Manifest = manifest;
        PluginDirectory = pluginDirectory;
        State = initialState;
        FailureReason = failureReason;
    }
    
    /// <summary>
    /// Creates a descriptor for a plugin directory whose manifest could not be
    /// parsed at all, using a minimal placeholder manifest since none is
    /// available. The placeholder's <see cref="IPluginManifest.Id"/> is derived
    /// from the directory name, so the plugin is still identifiable in
    /// diagnostics and the Plugin Manager UI despite the parse failure.
    /// </summary>
    /// <param name="pluginDirectory">The absolute path to the plugin's directory.</param>
    /// <param name="failureReason">A description of why the manifest failed to parse.</param>
    /// <returns>A new descriptor in the <see cref="PluginLoadState.Failed"/> state.</returns>
    public static PluginDescriptorImpl CreateFailed(string pluginDirectory, string failureReason)
    {
        var directoryName = System.IO.Path.GetFileName(pluginDirectory.TrimEnd('/', '\\'));
        var placeholderManifest = PlaceholderManifest.ForUnparsableDirectory(directoryName);

        return new PluginDescriptorImpl(placeholderManifest, pluginDirectory, PluginLoadState.Failed, failureReason);
    }

    /// <summary>
    /// Advances this descriptor to a new lifecycle state, clearing any prior
    /// failure reason unless the new state is itself <see cref="PluginLoadState.Failed"/>.
    /// </summary>
    /// <param name="newState">The state to transition to.</param>
    /// <param name="failureReason">The failure reason, required when <paramref name="newState"/> is <see cref="PluginLoadState.Failed"/>; ignored otherwise.</param>
    public void TransitionTo(PluginLoadState newState, string? failureReason = null)
    {
        State = newState;
        FailureReason = newState == PluginLoadState.Failed ? failureReason : null;
    }

    /// <summary>
    /// Records non-fatal warnings discovered about this plugin.
    /// </summary>
    /// <param name="warnings">The warnings to attach to this descriptor.</param>
    public void SetWarnings(IReadOnlyList<string> warnings)
    {
        Warnings = warnings;
    }
}