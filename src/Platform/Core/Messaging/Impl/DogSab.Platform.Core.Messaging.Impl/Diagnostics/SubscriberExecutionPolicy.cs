using DogSab.Platform.Core.Abstractions.Logging; 
using DogSab.Platform.Core.Abstractions.Messaging;

namespace DogSab.Platform.Core.Messaging.Impl.Diagnostics;

/// <summary>
/// Decides how the message bus reacts when one or more subscribers throw while
/// handling a published message. In all cases, every live subscriber is still
/// given a chance to run — one subscriber's failure must never prevent delivery
/// to the others. This policy only controls what happens to the exception(s)
/// after every subscriber has been attempted.
/// </summary>
public sealed class SubscriberExceptionPolicy
{
    /// <summary>Logger used to report subscriber failures regardless of the chosen behavior.</summary>
    private readonly ILogger _logger;

    /// <summary>Whether failures should be aggregated and rethrown to the publisher after delivery completes.</summary>
    private readonly bool _rethrowAfterDelivery;

    /// <summary>
    /// Creates a new exception policy.
    /// </summary>
    /// <param name="loggerFactory">Factory used to obtain a logger scoped to this policy.</param>
    /// <param name="rethrowAfterDelivery">
    /// If <c>true</c>, a <see cref="MessageDeliveryException"/> aggregating all subscriber
    /// failures is thrown back to the publishing code once every subscriber has been attempted.
    /// If <c>false</c> (the default), failures are only logged and the publisher never sees them —
    /// matching how most event buses behave, since publishers should not need to know
    /// or care about unrelated subscriber implementations.
    /// </param>
    public SubscriberExceptionPolicy(ILoggerFactory loggerFactory, bool rethrowAfterDelivery = false)
    {
        _logger = loggerFactory.GetLogger(typeof(SubscriberExceptionPolicy));
        _rethrowAfterDelivery = rethrowAfterDelivery;
    }

    /// <summary>
    /// Records that a single subscriber threw while handling a message. Always logs
    /// the failure immediately; whether it is later rethrown depends on the policy's
    /// <see cref="_rethrowAfterDelivery"/> setting and is decided in <see cref="HandleCollectedFailures"/>.
    /// </summary>
    /// <param name="topic">The topic being published to when the failure occurred.</param>
    /// <param name="subscriber">The subscriber instance whose handler threw.</param>
    /// <param name="exception">The exception the subscriber threw.</param>
    public void RecordFailure(ITopic topic, object subscriber, Exception exception)
    {
        _logger.Error(
            "Subscriber '{0}' threw while handling topic '{1}'",
            exception,
            subscriber.GetType().FullName ?? subscriber.GetType().Name,
            topic.DisplayName);
    }

    /// <summary>
    /// Called once after every live subscriber of a topic has been attempted for a
    /// given publish call. If configured to rethrow, aggregates all collected
    /// failures into a single <see cref="MessageDeliveryException"/> and throws it;
    /// otherwise this is a no-op, since failures were already logged by <see cref="RecordFailure"/>.
    /// </summary>
    /// <param name="topic">The topic that was published to.</param>
    /// <param name="failures">The subscriber/exception pairs collected during this publish call, if any.</param>
    /// <exception cref="MessageDeliveryException">
    /// Thrown if the policy is configured to rethrow and <paramref name="failures"/> is non-empty.
    /// </exception>
    public void HandleCollectedFailures(ITopic topic, IReadOnlyList<(object Subscriber, Exception Exception)> failures)
    {
        if (!_rethrowAfterDelivery || failures.Count == 0)
        {
            return;
        }

        throw new MessageDeliveryException(topic, failures);
    }
}