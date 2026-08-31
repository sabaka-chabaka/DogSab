namespace DogSab.Platform.RunConfigurations.Abstractions;

/// <summary>
/// A handle to a single launched process, returned once
/// <see cref="ProcessRunner"/> has started an <see cref="IRunConfiguration"/>.
/// Lets the platform observe the process's output and lifecycle, and
/// request it be stopped, without exposing the underlying
/// <see cref="System.Diagnostics.Process"/> object directly — keeping the
/// concrete process-launching mechanism (currently
/// <c>System.Diagnostics.Process</c>-based) swappable behind this
/// abstraction, the same reasoning already applied throughout the platform
/// for every other "here's the real OS-level thing, but plugins/consumers
/// only see our own contract" boundary.
/// </summary>
public interface IRunProcessHandle
{
    /// <summary>
    /// The run configuration this process was launched from.
    /// </summary>
    RunConfigurationId ConfigurationId { get; }

    /// <summary>
    /// The process's current lifecycle state.
    /// </summary>
    RunState State { get; }

    /// <summary>
    /// The process's exit code, once it has stopped. <c>null</c> while
    /// <see cref="State"/> is <see cref="RunState.NotStarted"/> or
    /// <see cref="RunState.Running"/>, and also <c>null</c> if
    /// <see cref="State"/> is <see cref="RunState.Failed"/> — a failed
    /// launch never produced a process that could exit with a code at all.
    /// </summary>
    int? ExitCode { get; }

    /// <summary>
    /// Raised each time a line of output is produced by the process, on
    /// either its standard output or standard error stream.
    /// </summary>
    event Action<string>? OutputReceived;

    /// <summary>
    /// Raised when the process's state changes, e.g. from
    /// <see cref="RunState.Running"/> to <see cref="RunState.Stopped"/>.
    /// </summary>
    event Action<RunState>? StateChanged;

    /// <summary>
    /// Requests that the process be terminated. If the process has already
    /// stopped or failed to launch, this is a no-op.
    /// </summary>
    void Stop();
}