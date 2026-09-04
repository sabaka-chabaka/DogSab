using DogSab.Platform.Core.Abstractions.Lifecycle;
using DogSab.Platform.Core.Abstractions.Logging;
using DogSab.Platform.Core.Abstractions.Threading;
using DogSab.Platform.Extensibility.Abstractions.Attributes;

namespace DogSab.Platform.Ui.Shell.Startup;

/// <summary>
/// Platform startup activity that constructs and shows the main window.
/// Runs last among currently defined startup activities, since it depends on
/// actions and tool windows already being registered.
/// </summary>
[Extension("core.startupActivity")]
public sealed class UiShellStartupActivity : IStartupActivity
{
    private readonly MenuBarBuilder _menuBarBuilder;
    private readonly Actions.MainMenuGroupBuilder _mainMenuGroupBuilder;
    private readonly IUiThreadDispatcher _uiThreadDispatcher;
    private readonly ILoggerFactory _loggerFactory;

    public UiShellStartupActivity(
        MenuBarBuilder menuBarBuilder,
        Actions.MainMenuGroupBuilder mainMenuGroupBuilder,
        IUiThreadDispatcher uiThreadDispatcher,
        ILoggerFactory loggerFactory)
    {
        _menuBarBuilder = menuBarBuilder;
        _mainMenuGroupBuilder = mainMenuGroupBuilder;
        _uiThreadDispatcher = uiThreadDispatcher;
        _loggerFactory = loggerFactory;
    }

    /// <summary>Runs after actions and tool windows are registered.</summary>
    public int Order => 2200;

    public async Task RunActivityAsync(CancellationToken cancellationToken)
    {
        var logger = _loggerFactory.GetLogger(typeof(UiShellStartupActivity));

        await _uiThreadDispatcher.InvokeAsync(() =>
        {
            var window = new MainWindow();
            var mainMenuGroup = _mainMenuGroupBuilder.Build();
            var menu = _menuBarBuilder.Build(mainMenuGroup);
            var statusBar = new StatusBar();

            window.SetMenuBar(menu);
            window.SetStatusBar(statusBar);
            window.Show();

            logger.Info("Main window shown.");
        });
    }
}