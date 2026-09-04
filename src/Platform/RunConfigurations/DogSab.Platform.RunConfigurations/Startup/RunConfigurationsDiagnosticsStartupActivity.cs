using DogSab.Platform.Core.Abstractions.Lifecycle;
using DogSab.Platform.Core.Abstractions.Logging;
using DogSab.Platform.Extensibility.Abstractions.Attributes;
using DogSab.Platform.Extensibility.Abstractions.ExtensionPoints;
using DogSab.Platform.RunConfigurations.Abstractions.Events;

namespace DogSab.Platform.RunConfigurations.Startup;

/// <summary>
/// Platform startup activity that logs which run configuration types are
/// registered, mirroring the diagnostics pattern used throughout the platform.
/// </summary>
[Extension("core.startupActivity")]
public sealed class RunConfigDiagnosticsStartupActivity : IStartupActivity
{
    /// <summary>
    /// The platform's extension point registry, used to count registered
    /// configuration types.
    /// </summary>
    private readonly IExtensionPointRegistry _extensionPointRegistry;

    /// <summary>
    /// Factory used to obtain a logger for this activity's diagnostic report.
    /// </summary>
    private readonly ILoggerFactory _loggerFactory;

    /// <summary>
    /// Creates a new run configuration diagnostics startup activity.
    /// </summary>
    /// <param name="extensionPointRegistry">
    /// The registry to report on.
    /// </param>
    /// <param name="loggerFactory">
    /// Factory used to obtain a logger for the report.
    /// </param>
    public RunConfigDiagnosticsStartupActivity(IExtensionPointRegistry extensionPointRegistry, ILoggerFactory loggerFactory)
    {
        _extensionPointRegistry = extensionPointRegistry;
        _loggerFactory = loggerFactory;
    }

    /// <summary>
    /// Runs late, after plugin loading is expected to have already
    /// registered its run configuration types.
    /// </summary>
    public int Order => 2300;

    /// <summary>
    /// Logs how many run configuration types are currently registered.
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
        var logger = _loggerFactory.GetLogger(typeof(RunConfigDiagnosticsStartupActivity));
        var typeCount = _extensionPointRegistry.GetExtensions(RunExtensionPoints.RUN_CONFIGURATION_TYPE).Count;

        logger.Info("{0} run configuration type(s) registered.", typeCount);

        return Task.CompletedTask;
    }
}