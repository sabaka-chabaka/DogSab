namespace DogSab.Platform.Core.Application.Bootstrap;

/// <summary>
/// Identifies a discrete stage of <see cref="PlatformBootstrapper"/>'s startup
/// sequence, in the order they are expected to run. Used for structured
/// startup logging and for diagnosing exactly which stage a startup failure
/// occurred in, since an exception thrown mid-bootstrap otherwise gives no
/// indication of how much of the platform was already successfully initialized.
/// </summary>
public enum BootstrapPhase
{
    /// <summary>Building the logging subsystem — the very first phase, since every later phase logs through it.</summary>
    Logging,

    /// <summary>Constructing the read/write action manager and UI thread dispatcher.</summary>
    Threading,

    /// <summary>Constructing the message bus.</summary>
    Messaging,

    /// <summary>Constructing the settings store and path resolver.</summary>
    Settings,

    /// <summary>Constructing the application-level component manager.</summary>
    Components,

    /// <summary>Building the root dependency injection container and registering core services.</summary>
    ServiceContainer,

    /// <summary>Constructing the root disposable registry.</summary>
    DisposableRegistry,

    /// <summary>Discovering and loading plugins via the extensibility subsystem.</summary>
    Plugins,

    /// <summary>Running all registered <see cref="Abstractions.Lifecycle.IStartupActivity"/> instances.</summary>
    StartupActivities,

    /// <summary>Bootstrap has completed successfully and the application is ready for use.</summary>
    Ready
}