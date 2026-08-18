using DogSab.Platform.Core.Abstractions.Lifecycle;
using DogSab.Platform.Core.Abstractions.Logging;
using DogSab.Platform.Indexing.Building;
using DogSab.Platform.ProjectModel.Abstractions.Roots;
using DogSab.Platform.ProjectModel.Module;

namespace DogSab.Platform.Indexing.Startup;

/// <summary>
/// Platform startup activity that enqueues every file under the currently
/// loaded solution's content roots for initial indexing. Runs after
/// <c>ProjectModelDiagnosticsStartupActivity</c> (which confirms the project
/// model finished loading), so there is a solution structure to walk. Simply
/// enqueues work onto <see cref="IndexBuildScheduler"/> and returns
/// immediately — the actual indexing runs asynchronously on the background
/// task queue, so this activity does not block the rest of startup waiting
/// for indexing to finish; the platform becomes usable (in dumb mode) as soon
/// as the UI is up, with indexing catching up in the background.
/// </summary>
public sealed class IndexingStartupActivity : IStartupActivity
{
    private readonly ProjectModelManager _projectModelManager;
    private readonly IndexBuildScheduler _scheduler;
    private readonly ILoggerFactory _loggerFactory;

    public IndexingStartupActivity(
        ProjectModelManager projectModelManager,
        IndexBuildScheduler scheduler,
        ILoggerFactory loggerFactory)
    {
        _projectModelManager = projectModelManager;
        _scheduler = scheduler;
        _loggerFactory = loggerFactory;
    }

    /// <summary>Runs after the project model is loaded (Order 1800), so it starts right afterward.</summary>
    public int Order => 1900;

    /// <summary>
    /// Walks every module's content roots in the current solution and
    /// enqueues all files found under their <see cref="SourceRootType.Source"/>,
    /// <see cref="SourceRootType.Test"/>, and
    /// <see cref="SourceRootType.Generated"/> source
    /// folders — <see cref="SourceRootType.Excluded"/>
    /// and <see cref="SourceRootType.Resource"/> folders
    /// are skipped, since excluded folders are explicitly meant not to be
    /// indexed and resource files are not typically code-index targets
    /// (individual <c>IIndexExtension</c> implementations still decide
    /// per-file applicability via <c>AppliesTo</c> regardless).
    /// </summary>
    /// <param name="cancellationToken">Token signaled if startup is aborted.</param>
    /// <returns>A completed task — indexing itself proceeds asynchronously in the background.</returns>
    public Task RunActivityAsync(CancellationToken cancellationToken)
    {
        var logger = _loggerFactory.GetLogger(typeof(IndexingStartupActivity));
        var solution = _projectModelManager.CurrentSolution;
        var enqueuedCount = 0;

        foreach (var project in solution.Projects)
        {
            foreach (var module in project.Modules)
            {
                foreach (var contentRoot in module.ContentRoots)
                {
                    foreach (var sourceFolder in contentRoot.SourceFolders)
                    {
                        if (sourceFolder.Type is SourceRootType.Excluded or SourceRootType.Resource)
                        {
                            continue;
                        }

                        enqueuedCount += EnqueueDirectoryRecursively(sourceFolder.Directory);
                    }
                }
            }
        }

        logger.Info("Initial indexing started: {0} file(s) enqueued.", enqueuedCount);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Recursively walks a directory, enqueuing every file (not subdirectory)
    /// found under it for indexing.
    /// </summary>
    /// <param name="directory">The directory to walk.</param>
    /// <returns>The number of files enqueued.</returns>
    private int EnqueueDirectoryRecursively(Vfs.Abstractions.VirtualFile.IVirtualFile directory)
    {
        var count = 0;

        foreach (var child in directory.GetChildren())
        {
            if (child.Type == Vfs.Abstractions.VirtualFile.VirtualFileType.Directory)
            {
                count += EnqueueDirectoryRecursively(child);
            }
            else
            {
                _scheduler.EnqueueFile(child);
                count++;
            }
        }

        return count;
    }
}