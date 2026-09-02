using DogSab.Platform.Core.Abstractions.Messaging;
using DogSab.Platform.Core.Messaging.Impl.Topics;
using DogSab.Platform.Extensibility.Abstractions.ExtensionPoints;

namespace DogSab.Platform.Debugger.Abstractions.Events;

/// <summary>
/// Declares the platform's debug process provider extension point and
/// debug session lifecycle topic.
/// </summary>
public static class DebuggerExtensionPoints
{
    /// <summary>
    /// Contributes a debug process provider for a specific
    /// runtime/protocol.
    /// Application-scoped — a debugger plugin registers its provider once
    /// for the whole process, not per open project.
    /// </summary>
    public static readonly ExtensionPointName<IDebugProcessProvider> DEBUG_PROCESS_PROVIDER =
        ExtensionPointName<IDebugProcessProvider>.Create(
            "debugger.processProvider",
            "Starts and drives debug sessions for a specific runtime/protocol.");

    /// <summary>
    /// Published whenever any active debug session's state changes.
    /// Delivered on the UI thread — a debug session pausing at a
    /// breakpoint is exactly the kind of event that must immediately
    /// update UI (jump the editor to the paused line, populate the call
    /// stack panel), and unlike file-change events, debug state
    /// transitions occur relatively infrequently (a handful of times per
    /// debugging step, not per keystroke), so UI-thread delivery here
    /// carries none of the blocking risk that ruled it out for
    /// high-frequency events elsewhere in the platform.
    /// </summary>
    public static readonly ITopic<IDebugSessionListener> DEBUG_SESSION_STATE_CHANGED =
        TopicImpl<IDebugSessionListener>.Create("debugger.sessionStateChanged", DeliveryMode.UiThread);
}