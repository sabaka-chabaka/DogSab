namespace DogSab.Platform.Extensibility.Abstractions.Sandbox;

/// <summary>
/// A capability a plugin may declare it needs in its manifest, for future use
/// by a plugin sandboxing mechanism that has not yet been implemented (see
/// <c>DogSab.Platform.Extensibility.Sandbox</c> in the plugin system's loading
/// pipeline). Declaring this now allows plugin manifests written today to
/// state their required permissions, so that when sandboxing enforcement is
/// added later, existing plugins do not need a manifest format change —
/// enforcement can simply start being applied to declarations that already exist.
/// </summary>
public enum PluginPermission
{
    /// <summary>The plugin needs to read and/or write files outside of the current project's directory.</summary>
    FileSystemAccess,

    /// <summary>The plugin needs to make outbound network requests.</summary>
    NetworkAccess,

    /// <summary>The plugin needs to start external processes (e.g. invoking a compiler or CLI tool).</summary>
    ProcessExecution
}