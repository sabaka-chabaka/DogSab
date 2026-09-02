namespace DogSab.Platform.Debugger.Abstractions;

/// <summary>
/// A single breakpoint set by the user at a specific file and line,
/// managed by <c>BreakpointManagerImpl</c> and persisted across sessions so
/// breakpoints survive closing and reopening a project.
/// A breakpoint exists independently of any particular
/// <see cref="IDebugSession"/> — it can be set before any debugging starts,
/// and remains set after a session ends, ready to be hit again the next
/// time debugging starts.
/// </summary>
public interface IBreakpoint
{
    /// <summary>
    /// This breakpoint's stable identifier.
    /// </summary>
    BreakpointId Id { get; }

    /// <summary>
    /// The file path this breakpoint is set in.
    /// </summary>
    string FilePath { get; }

    /// <summary>
    /// The one-based line number within <see cref="FilePath"/> this
    /// breakpoint is set at.
    /// </summary>
    int LineNumber { get; }

    /// <summary>
    /// Whether this breakpoint is currently enabled. A disabled breakpoint
    /// remains defined (its location is remembered) but does not cause
    /// execution to pause when reached — distinct from deleting it
    /// outright, which a user might not want to do if they expect to
    /// re-enable it shortly.
    /// </summary>
    bool IsEnabled { get; }

    /// <summary>
    /// An optional boolean expression, evaluated in the debuggee's context
    /// each time this breakpoint's location is reached; execution only
    /// actually pauses if the expression evaluates to true. <c>null</c> or
    /// empty means this is an unconditional breakpoint — the platform
    /// itself does not parse or validate this expression, since expression
    /// syntax is entirely language/runtime-specific; it is passed through
    /// verbatim to whichever <see cref="IDebugProcessProvider"/> is
    /// driving the active session.
    /// </summary>
    string? ConditionExpression { get; }
}