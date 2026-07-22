using DogSab.Platform.Core.Abstractions.Logging;
using DogSab.Platform.Core.Abstractions.Messaging;
using DogSab.Platform.Core.Abstractions.Progress;
using DogSab.Platform.Core.Abstractions.Settings;
using DogSab.Platform.Core.Abstractions.Threading;
using DogSab.Platform.Core.Impl.Components;
using DogSab.Platform.Core.Impl.Disposables;
using DogSab.Platform.Core.Impl.Lifecycle;
using DogSab.Platform.Core.Impl.Services;

namespace DogSab.Platform.Core.Application.Bootstrap;

/// <summary>
/// The complete set of top-level objects produced by a successful
/// <see cref="PlatformBootstrapper.Run"/> call, handed to
/// <see cref="Application.DogSabApplication"/> to expose as the platform's public API.
/// </summary>
public sealed class BootstrapResult
{
    public ILoggerFactory LoggerFactory { get; }
    public IReadWriteActionManager ReadWriteActionManager { get; }
    public IUiThreadDispatcher UiThreadDispatcher { get; }
    public IBackgroundTaskQueue BackgroundTaskQueue { get; }
    public IMessageBus MessageBus { get; }
    public ISettingsStore SettingsStore { get; }
    public IProgressManager ProgressManager { get; }
    public ApplicationComponentManager ApplicationComponentManager { get; }
    public DisposableRegistryImpl DisposableRegistry { get; }
    public ServiceContainerImpl RootServiceContainer { get; }
    public StartupActivityRunner StartupActivityRunner { get; }

    public BootstrapResult(
        ILoggerFactory loggerFactory,
        IReadWriteActionManager readWriteActionManager,
        IUiThreadDispatcher uiThreadDispatcher,
        IBackgroundTaskQueue backgroundTaskQueue,
        IMessageBus messageBus,
        ISettingsStore settingsStore,
        IProgressManager progressManager,
        ApplicationComponentManager applicationComponentManager,
        DisposableRegistryImpl disposableRegistry,
        ServiceContainerImpl rootServiceContainer,
        StartupActivityRunner startupActivityRunner)
    {
        LoggerFactory = loggerFactory;
        ReadWriteActionManager = readWriteActionManager;
        UiThreadDispatcher = uiThreadDispatcher;
        BackgroundTaskQueue = backgroundTaskQueue;
        MessageBus = messageBus;
        SettingsStore = settingsStore;
        ProgressManager = progressManager;
        ApplicationComponentManager = applicationComponentManager;
        DisposableRegistry = disposableRegistry;
        RootServiceContainer = rootServiceContainer;
        StartupActivityRunner = startupActivityRunner;
    }
}