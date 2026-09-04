using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using DogSab.Platform.Core.Application.Application;
using DogSab.Platform.Extensibility.Abstractions.ExtensionPoints;
using DogSab.Platform.Ui.Actions;
using DogSab.Platform.Ui.Actions.Abstractions;
using DogSab.Platform.Ui.Shell;

namespace DogSab.Platform.Host;

/// <summary>
/// The Avalonia application object.
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
            var actionRegistry = (IExtensionPointRegistry)DogSabApplication.Instance.RootServiceContainer
                .GetService(typeof(IExtensionPointRegistry));

            if (!actionRegistry.IsExtensionPointDeclared(ActionExtensionPoints.ACTION.Id))
            {
                actionRegistry.RegisterExtensionPoint(ActionExtensionPoints.ACTION, ExtensionPointArea.Application);
            }

            var actionManager = new ActionManagerImpl(actionRegistry);
            var activeProjectTracker = new ActiveProjectTracker();
            var menuBarBuilder = new MenuBarBuilder(actionManager, activeProjectTracker);
            var mainMenuGroupBuilder = new MainMenuGroupBuilder(actionManager);
            var mainMenuGroup = mainMenuGroupBuilder.Build();
            var menu = menuBarBuilder.Build(mainMenuGroup);
            var statusBar = new StatusBar();

            window.SetMenuBar(menu);
            window.SetStatusBar(statusBar);

            desktop.MainWindow = window;
        }

        base.OnFrameworkInitializationCompleted();
    }
}