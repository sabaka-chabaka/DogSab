using DogSab.Platform.RunConfigurations.Abstractions;

namespace DogSab.Platform.Debugger.Abstractions;

/// <summary>
/// Starts an <see cref="IDebugSession"/> for a given run configuration,
/// implemented per runtime/protocol (e.g. a future
/// <c>NetCoreDbgProvider</c> plugin drives .NET debuggees via netcoredbg's
/// Debug Adapter Protocol implementation) and registered against
/// <see cref="Events.DebuggerExtensionPoints.DEBUG_PROCESS_PROVIDER"/>.
/// Mirrors <c>RunConfigurations.Abstractions.IRunConfigurationType</c>'s
/// role for plain runs — the platform has no built-in knowledge of how to
/// actually start a debuggee under any particular debugger; that
/// entirely belongs to whichever provider is registered for a given
/// runtime.
/// </summary>
public interface IDebugProcessProvider
{
    /// <summary>
    /// A stable identifier for this provider (e.g. <c>"netcoredbg"</c>),
    /// used to select which provider should handle debugging a given run
    /// configuration.
    /// </summary>
    string ProviderId { get; }

    /// <summary>
    /// Determines whether this provider can debug the target described by
    /// a given run configuration — e.g. a .NET-specific provider only
    /// applies to configurations targeting .NET modules, not modules
    /// written in an unrelated language.
    /// </summary>
    /// <param name="configuration">
    /// The run configuration to check applicability for.
    /// </param>
    /// <returns>
    /// <c>true</c> if this provider can start a debug session for this
    /// configuration; otherwise <c>false</c>.
    /// </returns>
    bool CanDebug(IRunConfiguration configuration);

    /// <summary>
    /// Starts a new debug session for a run configuration, launching the
    /// debuggee under this provider's underlying debugger and returning
    /// once the session is ready to receive commands (though not
    /// necessarily yet paused — the debuggee typically starts running
    /// immediately and only pauses once a breakpoint is hit).
    /// </summary>
    /// <param name="configuration">
    /// The run configuration describing what to debug.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel session startup before it completes.
    /// </param>
    /// <returns>
    /// A task producing the started debug session.
    /// </returns>
    Task<IDebugSession> StartSessionAsync(IRunConfiguration configuration, CancellationToken cancellationToken);
}