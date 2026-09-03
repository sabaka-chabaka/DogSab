using Avalonia;
using DogSab.Platform.Core.Application.EntryPoints;
using DogSab.Platform.Host.CrashReporting;

namespace DogSab.Platform.Host;

/// <summary>
/// The platform's single process entry point. Deliberately thin — every
/// real subsystem (logging, threading, messaging, extensibility, project
/// model, indexing, PSI, UI shell) is already fully built and wired
/// together by <see cref="ApplicationBuilder.Start"/> in
/// <c>Core.Application</c>; this file's only job is to call that, hand
/// control to Avalonia's application lifetime, and ensure an orderly
/// shutdown happens no matter how the process exits.
/// </summary>
public static class Program
{
    /// <summary>
    /// The process entry point.
    /// </summary>
    /// <param name="args">
    /// Command-line arguments, forwarded to Avalonia's lifetime and
    /// available for a future <see cref="CommandLine.CliArgumentsParser"/>
    /// to interpret (e.g. a project path to open on startup) — not yet
    /// implemented here.
    /// </param>
    [STAThread]
    public static void Main(string[] args)
    {
        AppDomain.CurrentDomain.UnhandledException += CrashHandler.OnUnhandledException;

        var running = ApplicationBuilder.Start();

        try
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        finally
        {
            running.ShutdownCoordinator.Shutdown(
                Core.Application.Shutdown.ShutdownReason.UserRequested);
        }
    }

    /// <summary>
    /// Configures the Avalonia application builder.
    /// </summary>
    /// <returns>
    /// The configured <see cref="AppBuilder"/>.
    /// </returns>
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
}