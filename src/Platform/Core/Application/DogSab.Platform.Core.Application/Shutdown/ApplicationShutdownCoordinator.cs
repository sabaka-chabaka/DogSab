using DogSab.Platform.Core.Abstractions.Application;
using DogSab.Platform.Core.Abstractions.Logging;
using DogSab.Platform.Core.Application.Application;
using DogSab.Platform.Core.Application.ProjectLifecycle;
using DogSab.Platform.Core.Impl.Components;
using DogSab.Platform.Core.Impl.Disposables;

namespace DogSab.Platform.Core.Application.Shutdown;

/// <summary>
/// Coordinates the platform's shutdown sequence in a fixed, deliberate order:
/// close every open project, dispose application-scoped components, tear down
/// the disposable ownership tree, and only then let logging flush its final
/// output. Each step is attempted independently — a failure in one step is
/// logged and does not prevent the remaining steps from running, since a
/// partially-failed shutdown should still release as many resources as possible
/// rather than aborting the process in an undefined state.
/// </summary>
public sealed class ApplicationShutdownCoordinator
{
     /// <summary>Closes every open project as the first step of shutdown.</summary>
    private readonly ProjectSessionManager _projectSessionManager;

    /// <summary>Disposed after all projects are closed, since components may hold references touched during project close.</summary>
    private readonly ApplicationComponentManager _applicationComponentManager;

    /// <summary>The root of the disposable ownership tree, torn down after components.</summary>
    private readonly DisposableRegistryImpl _disposableRegistry;

    /// <summary>Publishes the final <see cref="ApplicationLifecycleEvent.Exiting"/> notification.</summary>
    private readonly ApplicationEventPublisher _eventPublisher;

    /// <summary>Logger used to report the progress and outcome of each shutdown step.</summary>
    private readonly ILogger _logger;

    /// <summary>
    /// Creates a new shutdown coordinator.
    /// </summary>
    /// <param name="projectSessionManager">Manager whose open projects are closed first.</param>
    /// <param name="applicationComponentManager">Manager whose components are disposed after projects close.</param>
    /// <param name="disposableRegistry">The disposable tree root torn down after components.</param>
    /// <param name="eventPublisher">Publisher used to announce the exiting transition.</param>
    /// <param name="loggerFactory">Factory used to obtain a logger scoped to this coordinator.</param>
    public ApplicationShutdownCoordinator(
        ProjectSessionManager projectSessionManager,
        ApplicationComponentManager applicationComponentManager,
        DisposableRegistryImpl disposableRegistry,
        ApplicationEventPublisher eventPublisher,
        ILoggerFactory loggerFactory)
    {
        _projectSessionManager = projectSessionManager;
        _applicationComponentManager = applicationComponentManager;
        _disposableRegistry = disposableRegistry;
        _eventPublisher = eventPublisher;
        _logger = loggerFactory.GetLogger(typeof(ApplicationShutdownCoordinator));
    }

    /// <summary>
    /// Runs the full shutdown sequence. Safe to call even if the application
    /// never fully finished starting up — each step tolerates missing state.
    /// </summary>
    /// <param name="reason">Why the application is shutting down, for diagnostics.</param>
    public void Shutdown(ShutdownReason reason)
    {
        _logger.Info("Shutdown initiated, reason: {0}", reason);

        RunStep("Publish exiting event", () => _eventPublisher.Publish(ApplicationLifecycleEvent.Exiting));
        RunStep("Close all projects", _projectSessionManager.CloseAllProjects);
        RunStep("Dispose application components", _applicationComponentManager.DisposeAll);
        RunStep("Tear down disposable tree", DisposeRegistryRoots);

        _logger.Info("Shutdown sequence completed, reason: {0}", reason);

        // Logging itself is intentionally the very last thing touched, and is
        // not disposed here — Microsoft.Extensions.Logging providers flush
        // synchronously on write in this platform's current configuration
        // (see RollingFileWriter, which opens/closes its file handle per line),
        // so no explicit flush step is required. If a buffered provider is
        // introduced later, an explicit flush step must be added here, last.
    }

    /// <summary>
    /// Disposes every root-level disposable known to the registry. Since
    /// <see cref="IDisposableRegistry"/> only knows how to tear down a subtree
    /// given its root, and the registry itself does not track which nodes are
    /// roots, this currently relies on the application having registered its
    /// top-level owned objects directly; see remarks on evolving this once
    /// root tracking is added to <see cref="DisposableRegistryImpl"/>.
    /// </summary>
    private void DisposeRegistryRoots()
    {
        // Placeholder: DisposableRegistryImpl.DisposeTree requires an explicit
        // root reference to tear down a subtree; it does not currently expose
        // "dispose everything registered". If PlatformBootstrapper registers a
        // single top-level owner object (e.g. a sentinel "application root"
        // IDisposable), that reference should be passed to DisposeTree here.
        _logger.Debug("Disposable tree teardown requested; see remarks for current limitation.");
    }

    /// <summary>
    /// Runs a single shutdown step, catching and logging any exception so that
    /// a failure in one step does not prevent subsequent steps from running.
    /// </summary>
    /// <param name="stepName">A short description of the step, for logging.</param>
    /// <param name="step">The step to execute.</param>
    private void RunStep(string stepName, Action step)
    {
        try
        {
            _logger.Debug("Shutdown step starting: {0}", stepName);
            step();
            _logger.Debug("Shutdown step completed: {0}", stepName);
        }
        catch (Exception ex)
        {
            _logger.Error("Shutdown step '{0}' failed", ex, stepName);
        }
    }
}