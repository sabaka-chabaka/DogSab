using DogSab.Platform.Core.Abstractions.Lifecycle;
using DogSab.Platform.Core.Abstractions.Logging;
using DogSab.Platform.Extensibility.Abstractions.ExtensionPoints;
using DogSab.Platform.Extensibility.Registry;

namespace DogSab.Platform.Extensibility.Startup;

/// <summary>
/// Platform startup activity that logs a summary of declared extension points
/// once plugin loading has completed, primarily to catch extension points that
/// ended up with zero registered implementations — often a sign of a plugin
/// that failed to load, or an extension point nobody has implemented yet.
/// Mirrors <c>MessagingDiagnosticsStartupActivity</c> from the Messaging module.
/// </summary>
public sealed class ExtensibilityDiagnosticsStartupActivity : IStartupActivity
{
    /// <summary>The registry whose declared extension points are reported.</summary>
    private readonly ExtensionPointRegistryImpl _registry;

    /// <summary>Factory used to obtain a logger for the diagnostic report.</summary>
    private readonly ILoggerFactory _loggerFactory;

    /// <summary>
    /// Creates a new extensibility diagnostics startup activity.
    /// </summary>
    /// <param name="registry">The registry to report on.</param>
    /// <param name="loggerFactory">Factory used to obtain a logger for the report.</param>
    public ExtensibilityDiagnosticsStartupActivity(ExtensionPointRegistryImpl registry, ILoggerFactory loggerFactory)
    {
        _registry = registry;
        _loggerFactory = loggerFactory;
    }

    /// <summary>
    /// Runs late, after plugin loading is expected to have already registered
    /// its extensions, so this reports on the final, settled state.
    /// </summary>
    public int Order => 2000;

    /// <summary>
    /// Logs how many extension points are known and (where determinable) flags
    /// application-scoped ones with zero registered implementations.
    /// </summary>
    /// <param name="cancellationToken">Token signaled if startup is aborted.</param>
    /// <returns>A completed task, since this activity performs only synchronous, in-memory work.</returns>
    public Task RunActivityAsync(CancellationToken cancellationToken)
    {
        var logger = _loggerFactory.GetLogger(typeof(ExtensibilityDiagnosticsStartupActivity));
        var snapshot = _registry.GetDiagnosticsSnapshot();

        logger.Debug("Extensibility diagnostics: {0} extension point(s) declared", snapshot.Count);

        foreach (var row in snapshot)
        {
            if (row.Area == ExtensionPointArea.Application && row.ApplicationScopeImplementationCount == 0)
            {
                logger.Warn(
                    "Extension point '{0}' has zero registered implementations — " +
                    "this may indicate a missing or failed plugin",
                    row.ExtensionPointId);
            }
            else if (row.Area == ExtensionPointArea.Application)
            {
                logger.Debug(
                    "Extension point '{0}' has {1} implementation(s)",
                    row.ExtensionPointId,
                    row.ApplicationScopeImplementationCount);
            }
            else
            {
                logger.Debug(
                    "Extension point '{0}' is project-scoped (per-project counts not shown here)",
                    row.ExtensionPointId);
            }
        }

        return Task.CompletedTask;
    }
}