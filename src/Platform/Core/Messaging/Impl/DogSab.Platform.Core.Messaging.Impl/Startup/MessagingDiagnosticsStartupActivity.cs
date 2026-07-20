using DogSab.Platform.Core.Abstractions.Lifecycle;
using DogSab.Platform.Core.Abstractions.Logging;
using DogSab.Platform.Core.Messaging.Impl.Bus;

namespace DogSab.Platform.Core.Messaging.Impl.Startup;

/// <summary>
/// Platform startup activity that logs a diagnostic summary of message bus
/// subscriptions shortly after startup, primarily to catch topics that ended pup
/// with zero subscribers due to a plugin initialization ordering bug or a
/// forgotten subscription. Intended for development/debug configuration
/// see <see cref="Order"/> remrarks on when it runs relative to other activities.
/// </summary>
public class MessagingDiagnosticsStartupActivity : IStartupActivity
{
    /// <summary>The registry whose subscription state is reported.</summary>
    private readonly TopicSubscriberRegistry _subscriberRegistry;

    /// <summary>Factory used to obtain a logger for the diagnostic report.</summary>
    private readonly ILoggerFactory _loggerFactory;

    /// <summary>
    /// Creates a new messaging diagnostics startup activity.
    /// </summary>
    /// <param name="subscriberRegistry">The registry to report on.</param>
    /// <param name="loggerFactory">Factory used to obtain a logger for the report.</param>
    public MessagingDiagnosticsStartupActivity(TopicSubscriberRegistry subscriberRegistry, ILoggerFactory loggerFactory)
    {
        _subscriberRegistry = subscriberRegistry;
        _loggerFactory = loggerFactory;
    }

    /// <summary>
    /// Runs late relative to most other startup activities, so that plugins have
    /// had a chance to complete their own subscriptions before this reports on them.
    /// </summary>
    public int Order => 1000;

    /// <summary>
    /// Logs a summary of currently known topics and their live subscriber counts,
    /// flagging any topic that has zero subscribers as a potential misconfiguration.
    /// </summary>
    /// <param name="cancellationToken">Token signaled if startup is aborted.</param>
    /// <returns>A completed task, since this activity performs only synchronous, in-memory work.</returns>
    public Task RunActivityAsync(CancellationToken cancellationToken)
    {
        var logger = _loggerFactory.GetLogger(typeof(MessagingDiagnosticsStartupActivity));
        var snapshot = _subscriberRegistry.GetDiagnosticsSnapshot();
        
        logger.Debug("Message bus diagnostics: {0} topic(s) known at startup", snapshot.Count);

        foreach (var (topic, liveSubscriberCount) in snapshot)
        {
            if (liveSubscriberCount == 0)
            {
                logger.Warn("Topic '{0}' has zero live subscribers at startup - this may indicate a missed subscription or initialization ordering issue", topic.DisplayName);
            }
            else
            {
                logger.Debug("Topic '{0}' has {1} live subscriber(s)", topic, liveSubscriberCount);
            }
        }
        
        return Task.CompletedTask;
    }
}