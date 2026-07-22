using DogSab.Platform.Core.Abstractions.Application;
using DogSab.Platform.Core.Abstractions.Logging;
using DogSab.Platform.Core.Abstractions.Messaging;
using DogSab.Platform.Core.Abstractions.Progress;
using DogSab.Platform.Core.Abstractions.Services;
using DogSab.Platform.Core.Abstractions.Settings;
using DogSab.Platform.Core.Abstractions.Threading;
using DogSab.Platform.Core.Application.Application;
using DogSab.Platform.Core.Impl.Components;
using DogSab.Platform.Core.Impl.DependencyInjection;
using DogSab.Platform.Core.Impl.Disposables;
using DogSab.Platform.Core.Impl.Lifecycle;
using DogSab.Platform.Core.Logging.Impl.Bootstrap;
using DogSab.Platform.Core.Messaging.Impl.Bus;
using DogSab.Platform.Core.Progress.Impl.Manager;
using DogSab.Platform.Core.Settings.Impl.Paths;
using DogSab.Platform.Core.Settings.Impl.Store;
using DogSab.Platform.Core.Threading.Impl.Background;
using DogSab.Platform.Core.Threading.Impl.ReadWrite;
using DogSab.Platform.Core.Threading.Impl.UiThread;

namespace DogSab.Platform.Core.Application.Bootstrap;

/// <summary>
/// Assembles every Core.*.Impl subsystem into a single, ready-to-use set of
/// application-level services, in the dependency order each subsystem requires.
/// This is the platform's single composition root: no other type is expected
/// to construct these Impl types directly. The result is handed to
/// <see cref="DogSabApplication"/>, which exposes it as the platform's public API.
/// </summary>
public sealed class PlatformBootstrapper
{
    /// <summary>Logger used to report bootstrap progress phase by phase.</summary>
    private ILogger _logger = null!;

    /// <summary>
    /// Runs the full bootstrap sequence and returns the assembled root container
    /// along with the other top-level objects <see cref="DogSabApplication"/> needs.
    /// </summary>
    /// <returns>The result of a successful bootstrap.</returns>
    /// <exception cref="Exception">
    /// Propagates any exception thrown during a phase, after logging which
    /// <see cref="BootstrapPhase"/> it occurred in.
    /// </exception>
    public BootstrapResult Run()
    {
        var loggerFactory = RunPhase(BootstrapPhase.Logging, () => LoggingBootstrapper.Build());
        _logger = loggerFactory.GetLogger(typeof(PlatformBootstrapper));

        var readWriteActionManager = RunPhase(
            BootstrapPhase.Threading,
            () => new ReadWriteActionManagerImpl(loggerFactory));

        var uiThreadDispatcher = RunPhase(
            BootstrapPhase.Threading,
            () => new AvaloniaUiThreadDispatcher(loggerFactory));

        var backgroundTaskQueue = RunPhase(
            BootstrapPhase.Threading,
            () => new BackgroundTaskQueueImpl(loggerFactory));

        var messageBus = RunPhase(
            BootstrapPhase.Messaging,
            () => new MessageBusImpl(uiThreadDispatcher, loggerFactory));

        var settingsStore = RunPhase(BootstrapPhase.Settings, () =>
        {
            // No project is open yet at this point in bootstrap — only
            // Application-scope settings can be resolved until a project opens.
            var pathResolver = new SettingsPathResolver(projectRootDirectory: null);
            return new SettingsStoreImpl(pathResolver, loggerFactory);
        });

        var progressManager = RunPhase(
            BootstrapPhase.Components, // grouped with Components since it has no dedicated phase of its own
            () => new ProgressManagerImpl());

        var applicationComponentManager = RunPhase(
            BootstrapPhase.Components,
            () => new ApplicationComponentManager(new ComponentDependencyResolver()));

        var disposableRegistry = RunPhase(
            BootstrapPhase.DisposableRegistry,
            () => new DisposableRegistryImpl(loggerFactory));

        var rootContainer = RunPhase(BootstrapPhase.ServiceContainer, () =>
        {
            var builder = new ContainerBuilder();
            var container = builder.Build();

            RegisterCoreServices(
                container,
                loggerFactory,
                readWriteActionManager,
                uiThreadDispatcher,
                backgroundTaskQueue,
                messageBus,
                settingsStore,
                progressManager);

            return container;
        });

        var startupActivityRunner = RunPhase(
            BootstrapPhase.StartupActivities,
            () => new StartupActivityRunner(loggerFactory, new LifecycleOrderResolver()));

        _logger.Info("Platform bootstrap phases complete; ready for startup activities and plugin loading");

        return new BootstrapResult(
            loggerFactory,
            readWriteActionManager,
            uiThreadDispatcher,
            backgroundTaskQueue,
            messageBus,
            settingsStore,
            progressManager,
            applicationComponentManager,
            disposableRegistry,
            rootContainer,
            startupActivityRunner);
    }

    /// <summary>
    /// Registers the already-constructed core singleton instances into the root
    /// service container under their public interface types, so the rest of the
    /// platform and all plugins can resolve them via <see cref="IServiceContainer"/>
    /// instead of receiving them through ad-hoc constructor wiring.
    /// </summary>
    private static void RegisterCoreServices(
        Impl.Services.ServiceContainerImpl container,
        ILoggerFactory loggerFactory,
        IReadWriteActionManager readWriteActionManager,
        IUiThreadDispatcher uiThreadDispatcher,
        IBackgroundTaskQueue backgroundTaskQueue,
        IMessageBus messageBus,
        ISettingsStore settingsStore,
        IProgressManager progressManager)
    {
        container.RegisterInstance<ILoggerFactory>(loggerFactory);
        container.RegisterInstance(readWriteActionManager);
        container.RegisterInstance(uiThreadDispatcher);
        container.RegisterInstance(backgroundTaskQueue);
        container.RegisterInstance(messageBus);
        container.RegisterInstance(settingsStore);
        container.RegisterInstance(progressManager);
        container.RegisterInstance(new ApplicationInfoImpl() as IApplicationInfo);
        container.RegisterInstance(new ApplicationEventPublisher(messageBus));
    }

    /// <summary>
    /// Runs a single bootstrap phase, logging its start and completion, and
    /// annotating any exception with the phase it occurred in before rethrowing.
    /// </summary>
    /// <typeparam name="T">The type produced by the phase.</typeparam>
    /// <param name="phase">The phase being executed, for logging and diagnostics.</param>
    /// <param name="action">The construction logic for this phase.</param>
    /// <returns>The value produced by <paramref name="action"/>.</returns>
    private T RunPhase<T>(BootstrapPhase phase, Func<T> action)
    {
        // _logger is not yet available during the very first (Logging) phase;
        // Console output is used as a fallback exactly as CoreDiagnosticsLogger does.
        _logger?.Debug("Bootstrap phase starting: {0}", phase);

        try
        {
            var result = action();
            _logger?.Debug("Bootstrap phase completed: {0}", phase);
            return result;
        }
        catch (Exception ex)
        {
            if (_logger is not null)
            {
                _logger.Error("Bootstrap phase '{0}' failed", ex, phase);
            }
            else
            {
                Console.Error.WriteLine($"[FATAL] Bootstrap phase '{phase}' failed before logging was available: {ex}");
            }

            throw;
        }
    }
}