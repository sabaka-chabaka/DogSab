using DogSab.Platform.Core.Abstractions.Lifecycle;
using DogSab.Platform.Core.Abstractions.Logging;
using DogSab.Platform.Debugger.Abstractions.Events;
using DogSab.Platform.Extensibility.Abstractions.Attributes;
using DogSab.Platform.Extensibility.Abstractions.ExtensionPoints;

namespace DogSab.Platform.Debugger.Startup;

/// <summary>
/// Platform startup activity that logs which debug process providers are
/// registered, mirroring the diagnostics pattern used throughout the platform.
/// </summary>
[Extension("core.startupActivity")]
public sealed class DebuggerDiagnosticsStartupActivity : IStartupActivity
{
    /// <summary>
    /// The platform's extension point registry, used to count registered
    /// debug process providers.
    /// </summary>
    private readonly IExtensionPointRegistry _extensionPointRegistry;

    /// <summary>
    /// Factory used to obtain a logger for this activity's diagnostic report.
    /// </summary>
    private readonly ILoggerFactory _loggerFactory;

    /// <summary>
    /// Creates a new debugger diagnostics startup activity.
    /// </summary>
    /// <param name="extensionPointRegistry">
    /// The registry to report on.
    /// </param>
    /// <param name="loggerFactory">
    /// Factory used to obtain a logger for the report.
    /// </param>
    public DebuggerDiagnosticsStartupActivity(IExtensionPointRegistry extensionPointRegistry, ILoggerFactory loggerFactory)
    {
        _extensionPointRegistry = extensionPointRegistry;
        _loggerFactory = loggerFactory;
    }

    /// <summary>
    /// Runs late, after plugin loading is expected to have already
    /// registered its debug process providers.
    /// </summary>
    public int Order => 2350;

    /// <summary>
    /// Logs how many debug process providers are currently registered.
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
        var logger = _loggerFactory.GetLogger(typeof(DebuggerDiagnosticsStartupActivity));
        var providerCount = _extensionPointRegistry.GetExtensions(DebuggerExtensionPoints.DEBUG_PROCESS_PROVIDER).Count;

        logger.Info("{0} debug process provider(s) registered.", providerCount);

        return Task.CompletedTask;
    }
}