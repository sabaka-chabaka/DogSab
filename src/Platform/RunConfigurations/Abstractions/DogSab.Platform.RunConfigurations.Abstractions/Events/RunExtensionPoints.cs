using DogSab.Platform.Core.Abstractions.Messaging;
using DogSab.Platform.Core.Messaging.Impl.Topics;
using DogSab.Platform.Extensibility.Abstractions.ExtensionPoints;

namespace DogSab.Platform.RunConfigurations.Abstractions.Events;

/// <summary>
/// Declares the platform's run configuration extension point and run
/// lifecycle topic.
/// </summary>
public static class RunExtensionPoints
{
    /// <summary>
    /// Contributes a kind of run configuration for a specific
    /// language/toolchain.
    /// Application-scoped — a toolchain plugin registers its
    /// configuration type once for the whole process, not per open project.
    /// </summary>
    public static readonly ExtensionPointName<IRunConfigurationType> RUN_CONFIGURATION_TYPE =
        ExtensionPointName<IRunConfigurationType>.Create(
            "runConfigurations.configurationType",
            "Provides a kind of run configuration and knows how to derive its actual launch command.");

    /// <summary>
    /// Published whenever any launched process's state changes. Delivered
    /// on the UI thread, since its primary subscribers are UI elements
    /// (a run output panel, a status indicator) reacting to state changes
    /// that occur comparatively infrequently — a process starting or
    /// stopping — unlike the high-frequency output line events, which
    /// remain on each individual <see cref="IRunProcessHandle.OutputReceived"/>
    /// rather than being funneled through this shared topic.
    /// </summary>
    public static readonly ITopic<IRunListener> RUN_STATE_CHANGED =
        TopicImpl<IRunListener>.Create("runConfigurations.stateChanged", DeliveryMode.UiThread);
}