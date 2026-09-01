namespace DogSab.Platform.Debugger.Abstractions;

/// <summary>
/// A single frame in the debuggee's call stack, captured while a debug
/// session is <see cref="DebugSessionState.Paused"/>.
/// Uses a plain file path and line number rather than a
/// <c>Vfs.Abstractions.VirtualFile.IVirtualFile</c>/<c>Psi.Abstractions.Tree.IPsiElement</c>
/// reference, since a debug adapter reports frame locations in its own
/// protocol's terms (a path string and line number from the underlying
/// runtime), and resolving that back to a platform <c>IVirtualFile</c> is
/// a separate concern for whichever UI displays this frame, not something
/// every debug adapter implementation should be required to do itself.
/// </summary>
public readonly struct StackFrame
{
    /// <summary>
    /// The name of the function/method this frame represents (e.g.
    /// <c>"MyApp.Program.Main"</c>).
    /// </summary>
    public string FunctionName { get; }

    /// <summary>
    /// The file path of the source location this frame is currently
    /// executing at, as reported by the debug adapter.
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    /// The one-based line number within <see cref="FilePath"/> this frame
    /// is currently executing at.
    /// </summary>
    public int LineNumber { get; }

    /// <summary>
    /// Creates a new stack frame.
    /// </summary>
    /// <param name="functionName">
    /// The name of the function/method this frame represents.
    /// </param>
    /// <param name="filePath">
    /// The source file path this frame is executing at.
    /// </param>
    /// <param name="lineNumber">
    /// The one-based line number within the file.
    /// </param>
    public StackFrame(string functionName, string filePath, int lineNumber)
    {
        FunctionName = functionName;
        FilePath = filePath;
        LineNumber = lineNumber;
    }
}