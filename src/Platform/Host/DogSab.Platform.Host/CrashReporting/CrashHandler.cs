namespace DogSab.Platform.Host.CrashReporting;

/// <summary>
/// Handles unhandled exceptions that escape all the way to the top of the
/// process, writing them to the console as a last-resort diagnostic before
/// the process terminates (the CLR always terminates the process after an
/// unhandled exception fires this event — this handler cannot prevent
/// that, only ensure something is recorded first).
/// A genuinely last-resort handler: by the time this fires, the platform's
/// own structured logging (<c>Core.Logging.Impl</c>) may itself be in an
/// unknown state, so this deliberately writes directly to
/// <see cref="Console"/> rather than attempting to resolve and use a
/// logger through the (possibly already-compromised) DI container.
/// </summary>
public static class CrashHandler
{
    /// <summary>
    /// Handles the <see cref="AppDomain.UnhandledException"/> event.
    /// </summary>
    /// <param name="sender">
    /// The event source, unused.
    /// </param>
    /// <param name="e">
    /// The unhandled exception event arguments.
    /// </param>
    public static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        Console.Error.WriteLine("=== DogSab: unhandled exception, process terminating ===");
        Console.Error.WriteLine(e.ExceptionObject);
        Console.Error.WriteLine("=========================================================");
    }
}