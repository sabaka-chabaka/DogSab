using DogSab.Platform.Core.Abstractions.Lifecycle;
using DogSab.Platform.Core.Abstractions.Logging;
using DogSab.Platform.Extensibility.Abstractions.Attributes;
using DogSab.Platform.Extensibility.Abstractions.ExtensionPoints;
using DogSab.Platform.Vcs.Abstractions.Events;

namespace DogSab.Platform.Vcs.Startup;

/// <summary>
/// Platform startup activity that logs which VCS providers are registered,
/// mirroring the diagnostics pattern used throughout the platform.
/// </summary>
[Extension("core.startupActivity")]
public sealed class VcsDiagnosticsStartupActivity : IStartupActivity
{
    /// <summary>
    /// The platform's extension point registry, used to count registered
    /// VCS providers.
    /// </summary>
    private readonly IExtensionPointRegistry _extensionPointRegistry;

    /// <summary>
    /// Factory used to obtain a logger for this activity's diagnostic report.
    /// </summary>
    private readonly ILoggerFactory _loggerFactory;

    /// <summary>
    /// Creates a new VCS diagnostics startup activity.
    /// </summary>
    /// <param name="extensionPointRegistry">
    /// The registry to report on.
    /// </param>
    /// <param name="loggerFactory">
    /// Factory used to obtain a logger for the report.
    /// </param>
    public VcsDiagnosticsStartupActivity(IExtensionPointRegistry extensionPointRegistry, ILoggerFactory loggerFactory)
    {
        _extensionPointRegistry = extensionPointRegistry;
        _loggerFactory = loggerFactory;
    }

    /// <summary>
    /// Runs late, after plugin loading is expected to have already
    /// registered its VCS providers.
    /// </summary>
    public int Order => 2400;

    /// <summary>
    /// Logs how many VCS providers are currently registered.
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
        var logger = _loggerFactory.GetLogger(typeof(VcsDiagnosticsStartupActivity));
        var providerCount = _extensionPointRegistry.GetExtensions(VcsExtensionPoints.VCS_PROVIDER).Count;

        logger.Info("{0} VCS provider(s) registered.", providerCount);

        return Task.CompletedTask;
    }
}