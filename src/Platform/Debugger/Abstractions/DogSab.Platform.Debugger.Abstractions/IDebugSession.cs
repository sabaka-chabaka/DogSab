namespace DogSab.Platform.Debugger.Abstractions;

/// <summary>
/// A single active debugging session — the debugging equivalent of
/// <c>RunConfigurations.Abstractions.IRunProcessHandle</c>, but additionally
/// exposing pause/resume/step control and inspection of the debuggee's
/// current call stack and variables while paused.
/// Created by an <see cref="IDebugProcessProvider"/> when a debug session
/// is started, and driven internally by a protocol-specific implementation
/// (e.g. <c>DapClient</c> for Debug Adapter Protocol-based runtimes) that
/// this interface deliberately says nothing about — callers interact only
/// through this contract's methods, never with the underlying wire protocol.
/// </summary>
public interface IDebugSession
{
    /// <summary>
    /// This session's current lifecycle state.
    /// </summary>
    DebugSessionState State { get; }

    /// <summary>
    /// Raised whenever <see cref="State"/> changes, e.g. from
    /// <see cref="DebugSessionState.Running"/> to
    /// <see cref="DebugSessionState.Paused"/> after a breakpoint is hit.
    /// </summary>
    event Action<DebugSessionState>? StateChanged;

    /// <summary>
    /// Raised each time a line of output is produced by the debuggee, on
    /// either its standard output or standard error stream.
    /// </summary>
    event Action<string>? OutputReceived;

    /// <summary>
    /// The debuggee's current call stack, from innermost to outermost
    /// frame. Only meaningful while <see cref="State"/> is
    /// <see cref="DebugSessionState.Paused"/>; returns an empty list
    /// otherwise.
    /// </summary>
    IReadOnlyList<StackFrame> GetCallStack();

    /// <summary>
    /// The variables visible within a given stack frame's scope. Only
    /// meaningful while <see cref="State"/> is
    /// <see cref="DebugSessionState.Paused"/>.
    /// </summary>
    /// <param name="frameIndex">
    /// The zero-based index into <see cref="GetCallStack"/>'s result
    /// identifying which frame's variables to retrieve.
    /// </param>
    /// <returns>
    /// The variables visible in that frame's scope.
    /// </returns>
    IReadOnlyList<Variable> GetVariables(int frameIndex);

    /// <summary>
    /// Resumes execution after being paused. No-op if not currently
    /// <see cref="DebugSessionState.Paused"/>.
    /// </summary>
    void Continue();

    /// <summary>
    /// Steps execution into the next function call on the current line, or
    /// to the next line if it contains no call. No-op if not currently
    /// paused.
    /// </summary>
    void StepInto();

    /// <summary>
    /// Steps execution over the current line, without descending into any
    /// function calls it contains. No-op if not currently paused.
    /// </summary>
    void StepOver();

    /// <summary>
    /// Steps execution out of the current function, resuming until it
    /// returns to its caller. No-op if not currently paused.
    /// </summary>
    void StepOut();

    /// <summary>
    /// Terminates the debuggee and ends this session.
    /// </summary>
    void Stop();
}