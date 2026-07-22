using DogSab.Platform.Core.Abstractions.Application;
using DogSab.Platform.Core.Abstractions.Components;
using DogSab.Platform.Core.Abstractions.Logging;
using DogSab.Platform.Core.Abstractions.Messaging;
using DogSab.Platform.Core.Abstractions.Progress;
using DogSab.Platform.Core.Abstractions.Services;
using DogSab.Platform.Core.Abstractions.Settings;
using DogSab.Platform.Core.Abstractions.Threading;
using DogSab.Platform.Core.Application.Bootstrap;
using DogSab.Platform.Core.Impl.Disposables;

namespace DogSab.Platform.Core.Application.Application;

/// <summary>
/// The platform's single application-level facade. Wraps the result of
/// <see cref="PlatformBootstrapper.Run"/> and exposes the core platform
/// services that both platform subsystems and plugins depend on. Exactly one
/// instance exists per process, created via <see cref="EntryPoints.ApplicationBuilder"/>
/// during startup and accessible thereafter via <see cref="Instance"/>.
/// </summary>
public sealed class DogSabApplication
{
    /// <summary>The single instance for the current process, set once by <see cref="Initialize"/>.</summary>
    private static DogSabApplication? _instance;

    /// <summary>The underlying bootstrap result this facade wraps.</summary>
    private readonly BootstrapResult _bootstrapResult;

    /// <summary>Publishes lifecycle transitions on <see cref="ApplicationEventPublisher.Topic"/>.</summary>
    private readonly ApplicationEventPublisher _eventPublisher;

    /// <summary>
    /// Creates the application facade. Internal — construction happens only
    /// through <see cref="Initialize"/> or for testing.
    /// </summary>
    /// <param name="bootstrapResult">The completed bootstrap result to wrap.</param>
    internal DogSabApplication(BootstrapResult bootstrapResult)
    {
        _bootstrapResult = bootstrapResult;
        _eventPublisher = bootstrapResult.RootServiceContainer.GetService<ApplicationEventPublisher>();
    }

    /// <summary>
    /// The current process's application instance. Throws if accessed before
    /// <see cref="Initialize"/> has completed.
    /// </summary>
    public static DogSabApplication Instance =>
        _instance ?? throw new InvalidOperationException(
            $"{nameof(DogSabApplication)} has not been initialized yet. Call {nameof(Initialize)} first.");

    /// <summary>Logger factory for obtaining category-scoped loggers.</summary>
    public ILoggerFactory LoggerFactory => _bootstrapResult.LoggerFactory;

    /// <summary>Manages read/write locking over the platform's shared model.</summary>
    public IReadWriteActionManager ReadWriteActionManager => _bootstrapResult.ReadWriteActionManager;

    /// <summary>Dispatches work onto the UI thread.</summary>
    public IUiThreadDispatcher UiThreadDispatcher => _bootstrapResult.UiThreadDispatcher;

    /// <summary>Schedules priority-ordered background work.</summary>
    public IBackgroundTaskQueue BackgroundTaskQueue => _bootstrapResult.BackgroundTaskQueue;

    /// <summary>The platform's publish/subscribe event bus.</summary>
    public IMessageBus MessageBus => _bootstrapResult.MessageBus;

    /// <summary>Persists and loads settings objects.</summary>
    public ISettingsStore SettingsStore => _bootstrapResult.SettingsStore;

    /// <summary>Runs and reports long-running operations.</summary>
    public IProgressManager ProgressManager => _bootstrapResult.ProgressManager;

    /// <summary>Manages the lifecycle of application-scoped components.</summary>
    public IComponentManager ApplicationComponentManager => _bootstrapResult.ApplicationComponentManager;

    /// <summary>The root of the platform's disposable ownership tree.</summary>
    public DisposableRegistryImpl DisposableRegistry => _bootstrapResult.DisposableRegistry;

    /// <summary>The root, application-scoped dependency injection container.</summary>
    public IServiceContainer RootServiceContainer => _bootstrapResult.RootServiceContainer;

    /// <summary>
    /// Runs the platform bootstrap sequence and sets <see cref="Instance"/>.
    /// Must be called exactly once per process, before any other platform code
    /// accesses <see cref="Instance"/>.
    /// </summary>
    /// <returns>The newly created and initialized application instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown if called more than once.</exception>
    public static DogSabApplication Initialize()
    {
        if (_instance is not null)
        {
            throw new InvalidOperationException($"{nameof(DogSabApplication)} has already been initialized.");
        }

        var bootstrapResult = new PlatformBootstrapper().Run();
        var application = new DogSabApplication(bootstrapResult);

        _instance = application;

        application._eventPublisher.Publish(ApplicationLifecycleEvent.Starting);
        application._eventPublisher.Publish(ApplicationLifecycleEvent.Started);

        return application;
    }

    /// <summary>
    /// Publishes the <see cref="ApplicationLifecycleEvent.Exiting"/> event.
    /// Actual resource teardown is performed separately by
    /// <see cref="Shutdown.ApplicationShutdownCoordinator"/>, which this method
    /// does not invoke itself, keeping event notification and teardown as
    /// distinct, independently testable steps.
    /// </summary>
    public void NotifyExiting()
    {
        _eventPublisher.Publish(ApplicationLifecycleEvent.Exiting);
    }
}