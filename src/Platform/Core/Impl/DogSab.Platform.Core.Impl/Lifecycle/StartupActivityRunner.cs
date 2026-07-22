using DogSab.Platform.Core.Abstractions.Lifecycle;
using DogSab.Platform.Core.Abstractions.Logging;

namespace DogSab.Platform.Core.Impl.Lifecycle;

/// <summary>
/// Discovers all <see cref="IStartupActivity"/> instances known to the platform
/// and runs them in ascending order of their declared <see cref="IStartupActivity.Order"/>,
/// sequentially, on a background thread.
/// </summary>
public class StartupActivityRunner
{
    /// <summary>Logger used to report progress and failures while running activities.</summary>
    private readonly ILogger _logger;
    
    /// <summary>busy</summary>
    private readonly LifecycleOrderResolver _orderResolver;

    /// <summary>Activities that have already completed successfully, in completion order.</summary>
    private readonly List<IStartupActivity> _completedActivities = new();

    /// <summary>
    /// Creates a new startup activity runner.
    /// </summary>
    /// <param name="loggerFactory">Factory used to obtain a logger scoped to this runner.</param>
    public StartupActivityRunner(ILoggerFactory loggerFactory, LifecycleOrderResolver orderResolver)
    {
        _logger = loggerFactory.GetLogger(typeof(StartupActivityRunner));
        _orderResolver = orderResolver;
    }

    /// <summary>Activities that have already completed successfully, in the order they finished.</summary>
    public IReadOnlyList<IStartupActivity> CompletedActivities => _completedActivities;

    /// <summary>
    /// Runs all given startup activities sequentially in ascending order of their
    /// declared <see cref="IStartupActivity.Order"/>. Activities with equal order
    /// run in the order they were supplied (stable ordering).
    /// </summary>
    /// <param name="activities">The set of startup activities to run.</param>
    /// <param name="cancellationToken">Token used to abort remaining activities.</param>
    /// <returns>A task that completes when all activities have finished or one has thrown.</returns>
    public async Task RunAllAsync(IEnumerable<IStartupActivity> activities, CancellationToken cancellationToken)
    {
        var ordered = _orderResolver.Resolve(activities, activity => activity.Order);

        foreach (var activity in ordered)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _logger.Debug("Running startup activity {0}", activity.GetType().FullName ?? activity.GetType().Name);

            try
            {
                await activity.RunActivityAsync(cancellationToken).ConfigureAwait(false);
                _completedActivities.Add(activity);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.Error(
                    "Startup activity {0} failed",
                    ex,
                    activity.GetType().FullName ?? activity.GetType().Name);
                throw;
            }
        }
    }
}