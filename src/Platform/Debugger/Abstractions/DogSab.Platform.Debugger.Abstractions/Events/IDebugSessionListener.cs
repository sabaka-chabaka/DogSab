namespace DogSab.Platform.Debugger.Abstractions.Events;

/// <summary>
/// Listener interface for debug session lifecycle notifications, published
/// on <see cref="DebuggerExtensionPoints.DEBUG_SESSION_STATE_CHANGED"/>.
/// Mirrors <c>RunConfigurations.Abstractions.Events.IRunListener</c>'s role
/// for plain runs — lets platform-wide subscribers (e.g. a Debug tool
/// window showing call stack/variables for whichever session is currently
/// active) observe every debug session without holding a direct reference
/// to each one.
/// </summary>
public interface IDebugSessionListener
{
    /// <summary>
    /// Called whenever any active debug session's state changes.
    /// </summary>
    /// <param name="session">
    /// The session whose state changed.
    /// </param>
    void DebugSessionStateChanged(IDebugSession session);
}