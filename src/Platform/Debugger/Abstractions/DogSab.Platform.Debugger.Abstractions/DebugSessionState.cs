namespace DogSab.Platform.Debugger.Abstractions;

/// <summary>
/// The current lifecycle state of an active debugging session.
/// A superset of <c>RunConfigurations.Abstractions.RunState</c>'s concerns
/// — a debug session is also a running process, but additionally has the
/// notion of being <see cref="Paused"/> at a breakpoint, which a plain run
/// has no equivalent for. Kept as a fully separate enum rather than trying
/// to extend or share <c>RunState</c>, since conflating "a plain launched
/// process" and "a debuggable session" into one type would force every
/// consumer of <c>RunState</c> to handle a <c>Paused</c> case that only
/// ever applies to debugging.
/// </summary>
public enum DebugSessionState
{
    /// <summary>
    /// The session has been configured but the debuggee has not yet started.
    /// </summary>
    NotStarted,

    /// <summary>
    /// The debuggee is currently executing.
    /// </summary>
    Running,

    /// <summary>
    /// Execution is currently suspended, e.g. at a hit breakpoint or after
    /// a step command, and the current call stack/variables can be inspected.
    /// </summary>
    Paused,

    /// <summary>
    /// The debuggee has exited and the session has ended.
    /// </summary>
    Stopped
}