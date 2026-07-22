namespace DogSab.Platform.Core.Application.Shutdown;

/// <summary>
/// Identifies why the application is shutting down, so
/// <see cref="ApplicationShutdownCoordinator"/> and any code observing shutdown
/// events can react appropriately — for example, skipping non-essential cleanup
/// steps during a crash to shut down as quickly as possible, or logging
/// differently depending on the cause.
/// </summary>
public enum ShutdownReason
{
    /// <summary>The user explicitly closed the application (e.g. via File &gt; Exit or closing the last window).</summary>
    UserRequested,

    /// <summary>Shutdown was triggered by an unhandled exception or other unrecoverable internal error.</summary>
    Crash,

    /// <summary>Shutdown was triggered by an external signal (e.g. SIGTERM, system shutdown, or an OS session ending).</summary>
    ExternalSignal
}