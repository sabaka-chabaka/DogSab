using DogSab.Platform.Core.Application.Application;
using DogSab.Platform.Core.Application.ProjectLifecycle;
using DogSab.Platform.Core.Application.Shutdown;

namespace DogSab.Platform.Core.Application.EntryPoints;

/// <summary>
/// Thin fluent entry point used by <c>DogSab.Platform.Host.Program</c> to start
/// and stop the platform without needing to know about
/// <see cref="Bootstrap.PlatformBootstrapper"/>, <see cref="DogSabApplication"/>,
/// or any other composition-root detail directly.
/// </summary>
public sealed class ApplicationBuilder
{
    /// <summary>
    /// Runs the full platform bootstrap sequence and returns a ready-to-use
    /// running application, including its project session manager and shutdown
    /// coordinator.
    /// </summary>
    /// <returns>The running application handle.</returns>
    public static RunningApplication Start()
    {
        var application = DogSabApplication.Initialize();

        var projectSessionManager = new ProjectSessionManager(
            (Impl.Services.ServiceContainerImpl)application.RootServiceContainer,
            application.LoggerFactory);

        var shutdownCoordinator = new ApplicationShutdownCoordinator(
            projectSessionManager,
            (Impl.Components.ApplicationComponentManager)application.ApplicationComponentManager,
            application.DisposableRegistry,
            application.RootServiceContainer.GetService<ApplicationEventPublisher>(),
            application.LoggerFactory);

        return new RunningApplication(application, projectSessionManager, shutdownCoordinator);
    }
}

/// <summary>
/// The result of <see cref="ApplicationBuilder.Start"/>: everything
/// <c>Program.cs</c> needs to open projects, run the UI, and shut down cleanly.
/// </summary>
public sealed class RunningApplication
{
    /// <summary>The platform's application facade.</summary>
    public DogSabApplication Application { get; }

    /// <summary>Manages opening and closing projects for the running session.</summary>
    public ProjectSessionManager ProjectSessionManager { get; }

    /// <summary>Coordinates orderly shutdown when the process is exiting.</summary>
    public ApplicationShutdownCoordinator ShutdownCoordinator { get; }

    /// <summary>
    /// Creates a new running application handle.
    /// </summary>
    /// <param name="application">The platform's application facade.</param>
    /// <param name="projectSessionManager">Manager for opening and closing projects.</param>
    /// <param name="shutdownCoordinator">Coordinator for orderly shutdown.</param>
    public RunningApplication(
        DogSabApplication application,
        ProjectSessionManager projectSessionManager,
        ApplicationShutdownCoordinator shutdownCoordinator)
    {
        Application = application;
        ProjectSessionManager = projectSessionManager;
        ShutdownCoordinator = shutdownCoordinator;
    }
}