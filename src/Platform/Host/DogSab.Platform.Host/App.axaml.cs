using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes; 
using Avalonia.Markup.Xaml;
using DogSab.Platform.Core.Abstractions.Logging;
using DogSab.Platform.Core.Abstractions.Messaging;
using DogSab.Platform.Core.Abstractions.Threading;
using DogSab.Platform.Core.Application.Application;
using DogSab.Platform.Core.Impl.Services;
using DogSab.Platform.Core.Messaging.Impl.Bus;
using DogSab.Platform.Extensibility.Abstractions.ExtensionPoints;
using DogSab.Platform.Extensibility.Reflection;
using DogSab.Platform.Indexing.Building;
using DogSab.Platform.Indexing.Dumb;
using DogSab.Platform.Indexing.Index;
using DogSab.Platform.ProjectModel.Abstractions.Project;
using DogSab.Platform.ProjectModel.Abstractions.Solution;
using DogSab.Platform.ProjectModel.Module;
using DogSab.Platform.ProjectModel.Solution;
using DogSab.Platform.Psi.Abstractions.Registry;
using DogSab.Platform.Psi.Registry;
using DogSab.Platform.Ui.Actions;
using DogSab.Platform.Ui.Actions.Abstractions;
using DogSab.Platform.Ui.Shell;
using DogSab.Platform.Ui.ToolWindows.Abstractions;
using DogSab.Platform.Vfs.FileSystem;

namespace DogSab.Platform.Host;

/// <summary>
/// The Avalonia application  object.
/// </summary>
public partial class App : Application
{
    /// <inheritdoc />
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    /// <inheritdoc />
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var window = new MainWindow();
            var container = DogSabApplication.Instance.RootServiceContainer;
            var logger = DogSabApplication.Instance.LoggerFactory.GetLogger(typeof(App));
            var registry = (IExtensionPointRegistry)container.GetService(typeof(IExtensionPointRegistry));

            // Declare the extension points owned by UI-layer modules. Core.Application
            // can't declare these itself during bootstrap without depending on Ui.Actions
            // and Ui.ToolWindows, which would invert the platform's layering — so, like
            // core.startupActivity is declared by Core.Application, each of these is
            // declared here by Host, the one place that legitimately sees every layer.
            if (!registry.IsExtensionPointDeclared(ActionExtensionPoints.ACTION.Id))
            {
                registry.RegisterExtensionPoint(ActionExtensionPoints.ACTION, ExtensionPointArea.Application);
            }

            if (!registry.IsExtensionPointDeclared(ToolWindowExtensionPoints.TOOL_WINDOW.Id))
            {
                registry.RegisterExtensionPoint(ToolWindowExtensionPoints.TOOL_WINDOW, ExtensionPointArea.Application);
            }

            // Register the handful of subsystem services that are simple enough to
            // construct safely here (no UI, no project-specific state beyond an
            // empty placeholder solution) so the corresponding [Extension]-attributed
            // diagnostics activities — and, for ProjectModelManager, real consumers
            // like ProjectViewToolWindowFactory — can actually be constructed by the
            // scan below instead of being skipped for a missing dependency.
            RegisterAdditionalServices((ServiceContainerImpl)container);

            // Actually discover and register every [Extension]-attributed class the
            // platform ships (actions, tool window factories, startup activities, ...).
            // Previously nothing did this at all — see InternalExtensionLoader's remarks.
            // Scans whatever DogSab.*.dll files exist next to the running executable;
            // Host's .csproj now references every module with an [Extension]-attributed
            // class so all of them are actually present here to be found.
            var assemblies = LoadAllPlatformAssemblies(logger);
            var extensionLoader = new InternalExtensionLoader(container, registry);
            var loadResult = extensionLoader.ScanAndRegister(assemblies);
            loadResult.LogTo(logger);

            var actionManager = new ActionManagerImpl(registry);
            var activeProjectTracker = new ActiveProjectTracker();
            var menuBarBuilder = new MenuBarBuilder(actionManager, activeProjectTracker);
            var mainMenuGroupBuilder = new MainMenuGroupBuilder(actionManager);
            var mainMenuGroup = mainMenuGroupBuilder.Build();
            var menu = menuBarBuilder.Build(mainMenuGroup);
            var statusBar = new StatusBar();
            var editorTabsHost = new EditorTabsHost();

            // FALLBACK: only kicks in if the real pipeline above still produced an
            // empty menu — e.g. because OpenFileAction/PluginManagerAction's own
            // dependency chains (EditorOpeningService, IPluginLoader, ...) aren't
            // registered in the container yet, so they were discovered but skipped
            // (see the warnings loadResult.LogTo just printed). Keeps the window
            // usable in the meantime instead of ending up with genuinely nothing.
            if (menu.Items.Count == 0)
            {
                var fileMenu = new MenuItem { Header = "_File" };
                var exitItem = new MenuItem { Header = "E_xit" };
                exitItem.Click += (_, _) => desktop.Shutdown();
                fileMenu.Items.Add(exitItem);
                menu.Items.Add(fileMenu);
            }

            // Same fallback rationale: with no project open and no tool window
            // successfully wired up yet, show something explicit rather than a
            // genuinely empty (if now visibly colored) TabControl.
            editorTabsHost.OpenTab("welcome", "Welcome", new TextBlock
            {
                Text = "DogSab IDE — no project open yet.",
                Margin = new Thickness(16),
                Foreground = Avalonia.Media.Brushes.White
            });

            window.SetMenuBar(menu);
            window.SetStatusBar(statusBar);
            window.SetEditorTabsHost(editorTabsHost);

            desktop.MainWindow = window;
        }

        base.OnFrameworkInitializationCompleted();
    }

    /// <summary>
    /// Constructs and registers a handful of subsystem services that are cheap
    /// and safe to build here: they need only already-registered core services,
    /// or nothing at all. This does NOT attempt to bootstrap the Vfs, Psi,
    /// ProjectModel, or Indexing subsystems for real — it exists purely so the
    /// platform's [Extension]-attributed diagnostics activities for those
    /// subsystems (and real consumers like ProjectViewToolWindowFactory, which
    /// needs a ProjectModelManager) can actually construct instead of being
    /// skipped. A genuinely working Vfs/Psi/ProjectModel/Indexing bootstrap —
    /// wiring real file system providers, real parser registrations, opening
    /// an actual solution file, etc. — is separate, larger work.
    /// </summary>
    private static void RegisterAdditionalServices(ServiceContainerImpl container)
    {
        // Vfs: an empty provider registry — no "file://" or other scheme provider
        // is registered against it yet, so it won't actually resolve any real path.
        container.RegisterInstance(new VirtualFileSystemRegistry());

        // Psi: an empty language registry — no parser for any language has been
        // registered against it yet, so it won't actually recognize any file type.
        container.RegisterInstance<ILanguageRegistry>(new LanguageRegistryImpl());

        // ProjectModel: a placeholder empty solution with no projects, standing in
        // for "no project is open" until a real "open solution" flow exists.
        var messageBus = (IMessageBus)container.GetService(typeof(IMessageBus));
        var placeholderSolution = new SolutionImpl(SolutionId.NewId(), "Untitled", Array.Empty<IProject>());
        container.RegisterInstance(new ProjectModelManager(placeholderSolution, messageBus));

        // Indexing: a real, working (if currently empty — no IIndexExtension has
        // been registered against it) build pipeline, since every piece it needs
        // (IndexRegistry, IndexBuildWorker, DumbServiceImpl) is itself simple
        // enough to construct safely.
        var indexRegistry = new IndexRegistry();
        var indexBuildWorker = new IndexBuildWorker(indexRegistry, (ILoggerFactory)container.GetService(typeof(ILoggerFactory)));
        var dumbService = new DumbServiceImpl();
        var backgroundTaskQueue = (IBackgroundTaskQueue)container.GetService(typeof(IBackgroundTaskQueue));
        container.RegisterInstance(new IndexBuildScheduler(
            indexBuildWorker,
            indexRegistry,
            dumbService,
            backgroundTaskQueue,
            messageBus,
            (ILoggerFactory)container.GetService(typeof(ILoggerFactory))));

        // Messaging: the message bus's own real subscriber registry, so
        // MessagingDiagnosticsStartupActivity reports genuine subscriber counts
        // instead of an unrelated, always-empty registry.
        container.RegisterInstance(((MessageBusImpl)messageBus).SubscriberRegistry);
    }

    /// <summary>
    /// Force-loads every "DogSab.*.dll" sitting next to the running executable and
    /// returns every currently loaded DogSab assembly. Force-loading is necessary
    /// because .NET only loads an assembly the first time something actually uses
    /// a type from it — merely having a project reference (and therefore the DLL
    /// present in the output folder) is not enough for it to show up in
    /// <see cref="AppDomain.GetAssemblies"/> on its own.
    /// </summary>
    private static IReadOnlyList<Assembly> LoadAllPlatformAssemblies(ILogger logger)
    {
        var baseDirectory = AppContext.BaseDirectory;

        var alreadyLoadedNames = AppDomain.CurrentDomain.GetAssemblies()
            .Select(a => a.GetName().Name)
            .Where(name => name is not null)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var dllPath in Directory.GetFiles(baseDirectory, "DogSab.*.dll"))
        {
            var simpleName = Path.GetFileNameWithoutExtension(dllPath);

            // Skip assemblies already loaded into this process (e.g. everything Host
            // directly references, and anything already touched during bootstrap) —
            // loading the same assembly again from disk would create a second, distinct
            // copy with its own type identity, breaking type checks for any type from
            // it used elsewhere (e.g. RegisterExtensionUntyped's contract-type check).
            if (alreadyLoadedNames.Contains(simpleName))
            {
                continue;
            }

            try
            {
                Assembly.LoadFrom(dllPath);
            }
            catch (Exception ex)
            {
                logger.Warn("Could not load assembly '{0}' for extension scanning: {1}", dllPath, ex.Message);
            }
        }

        return AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.GetName().Name?.StartsWith("DogSab.", StringComparison.Ordinal) == true)
            .ToList();
    }
}