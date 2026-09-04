using DogSab.Platform.Core.Abstractions.Lifecycle;
using DogSab.Platform.Core.Abstractions.Logging;
using DogSab.Platform.Extensibility.Abstractions.Attributes;
using DogSab.Platform.ProjectModel.Module;

namespace DogSab.Platform.ProjectModel.Startup;

/// <summary>
/// Platform startup activity that logs a summary of the loaded project model
/// — how many projects and modules are present — mirroring the diagnostics
/// pattern used throughout the platform (Messaging, Extensibility, Vfs).
/// </summary>
[Extension("core.startupActivity")]
public sealed class ProjectModelDiagnosticsStartupActivity : IStartupActivity
{
    private readonly ProjectModelManager _manager;
    private readonly ILoggerFactory _loggerFactory;

    public ProjectModelDiagnosticsStartupActivity(ProjectModelManager manager, ILoggerFactory loggerFactory)
    {
        _manager = manager;
        _loggerFactory = loggerFactory;
    }

    public int Order => 1800;

    public Task RunActivityAsync(CancellationToken cancellationToken)
    {
        var logger = _loggerFactory.GetLogger(typeof(ProjectModelDiagnosticsStartupActivity));
        var solution = _manager.CurrentSolution;
        var totalModules = solution.Projects.Sum(p => p.Modules.Count);

        logger.Info(
            "Project model loaded: solution '{0}' with {1} project(s), {2} module(s) total.",
            solution.DisplayName,
            solution.Projects.Count,
            totalModules);

        return Task.CompletedTask;
    }
}