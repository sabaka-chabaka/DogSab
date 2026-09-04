using DogSab.Platform.Core.Abstractions.Lifecycle;
using DogSab.Platform.Core.Abstractions.Logging;
using DogSab.Platform.Editor.Abstractions.Events;
using DogSab.Platform.Extensibility.Abstractions.Attributes;
using DogSab.Platform.Extensibility.Abstractions.ExtensionPoints;

namespace DogSab.Platform.Editor.Startup;

/// <summary>
/// Platform startup activity that logs how many completion providers,
/// folding providers, and inspections are currently registered, mirroring
/// the diagnostics pattern already used throughout the rest of the platform
/// for every other extension-point-driven module.
/// </summary>
[Extension("core.startupActivity")]
public sealed class EditorDiagnosticsStartupActivity : IStartupActivity
{
    /// <summary>
    /// The platform's extension point registry, used to count registered
    /// providers of each Editor extension point.
    /// </summary>
    private readonly IExtensionPointRegistry _extensionPointRegistry;

    /// <summary>
    /// Factory used to obtain a logger for this activity's diagnostic report.
    /// </summary>
    private readonly ILoggerFactory _loggerFactory;

    /// <summary>
    /// Creates a new Editor diagnostics startup activity.
    /// </summary>
    /// <param name="extensionPointRegistry">
    /// The registry to report on.
    /// </param>
    /// <param name="loggerFactory">
    /// Factory used to obtain a logger for the report.
    /// </param>
    public EditorDiagnosticsStartupActivity(IExtensionPointRegistry extensionPointRegistry, ILoggerFactory loggerFactory)
    {
        _extensionPointRegistry = extensionPointRegistry;
        _loggerFactory = loggerFactory;
    }

    /// <summary>
    /// Runs late, after plugin loading is expected to have already
    /// registered its Editor extensions.
    /// </summary>
    public int Order => 2150;

    /// <summary>
    /// Logs the number of currently registered completion providers,
    /// folding providers, and inspections.
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
        var logger = _loggerFactory.GetLogger(typeof(EditorDiagnosticsStartupActivity));

        var completionCount = _extensionPointRegistry.GetExtensions(EditorExtensionPoints.COMPLETION_PROVIDER).Count;
        var foldingCount = _extensionPointRegistry.GetExtensions(EditorExtensionPoints.FOLDING_PROVIDER).Count;
        var inspectionCount = _extensionPointRegistry.GetExtensions(EditorExtensionPoints.INSPECTION).Count;

        logger.Info(
            "Editor diagnostics: {0} completion provider(s), {1} folding provider(s), {2} inspection(s) registered.",
            completionCount,
            foldingCount,
            inspectionCount);

        return Task.CompletedTask;
    }
}