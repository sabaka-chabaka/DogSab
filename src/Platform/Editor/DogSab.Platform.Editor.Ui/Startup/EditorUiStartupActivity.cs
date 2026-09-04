using DogSab.Platform.Core.Abstractions.Lifecycle;
using DogSab.Platform.Core.Abstractions.Logging;
using DogSab.Platform.Extensibility.Abstractions.Attributes;

namespace DogSab.Platform.Editor.Ui.Startup;

/// <summary>
/// Platform startup activity for the Editor.Ui module.
/// Currently a diagnostics-only placeholder, mirroring the pattern used
/// throughout the platform for every other module — logs that the module
/// initialized successfully. Does not yet wire an actual "open file"
/// action into the platform's action system or tool window/tab
/// infrastructure, since that requires a concrete integration point (a
/// registered <c>AnAction</c> for "Open File", and a place to host
/// <see cref="EditorView"/> instances as editor tabs within
/// <c>Ui.Shell.MainWindow</c>) that has not been designed yet — see the
/// remark below.
/// </summary>
[Extension("core.startupActivity")]
public sealed class EditorUiStartupActivity : IStartupActivity
{
    /// <summary>
    /// Factory used to obtain a logger for this activity's diagnostic report.
    /// </summary>
    private readonly ILoggerFactory _loggerFactory;

    /// <summary>
    /// Creates a new Editor.Ui startup activity.
    /// </summary>
    /// <param name="loggerFactory">
    /// Factory used to obtain a logger for the report.
    /// </param>
    public EditorUiStartupActivity(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }

    /// <summary>
    /// Runs after <c>Ui.Shell</c>'s main window is expected to already be
    /// shown, since a future "open file" integration point would need the
    /// main window to exist to host editor tabs within it.
    /// </summary>
    public int Order => 2250;

    /// <summary>
    /// Logs that the Editor.Ui module has initialized.
    /// </summary>
    /// <param name="cancellationToken">
    /// Token signaled if startup is aborted.
    /// </param>
    /// <returns>
    /// A completed task, since this activity performs only synchronous,
    /// in-memory work.
    /// </returns>
    public Task RunActivityAsync(CancellationToken cancellationToken)
    {
        var logger = _loggerFactory.GetLogger(typeof(EditorUiStartupActivity));
        logger.Info("Editor.Ui module initialized.");

        return Task.CompletedTask;
    }
}