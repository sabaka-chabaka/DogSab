using DogSab.Platform.Core.Abstractions.Messaging;

namespace DogSab.Platform.Core.Messaging.Impl.Diagnostics;

/// <summary>
/// Aggregates one or more subscriber exceptions raised while delivering a single
/// published message, so a publisher configured with a rethrowing
/// <see cref="SubscriberExceptionPolicy"/> can observe all failures at once
/// rather than only the first one encountered.
/// </summary>
public sealed class MessageDeliveryException : Exception
{
    /// <summary>The topic being published to when the failures occurred.</summary>
    public ITopic Topic { get; }

    /// <summary>
    /// Every subscriber that failed, paired with the exception it threw, in the
    /// order subscribers were invoked.
    /// </summary>
    public IReadOnlyList<(object Subscriber, Exception Exception)> Failures { get; }

    /// <summary>
    /// Creates a new exception aggregating one or more subscriber delivery failures.
    /// </summary>
    /// <param name="topic">The topic being published to when the failures occurred.</param>
    /// <param name="failures">The subscriber/exception pairs that failed during delivery.</param>
    public MessageDeliveryException(ITopic topic, IReadOnlyList<(object Subscriber, Exception Exception)> failures)
        : base(BuildMessage(topic, failures))
    {
        Topic = topic;
        Failures = failures;
    }

    /// <summary>
    /// Builds a human-readable summary message listing how many subscribers failed
    /// and their types, for use as the exception's top-level <see cref="Exception.Message"/>.
    /// </summary>
    /// <param name="topic">The topic being published to when the failures occurred.</param>
    /// <param name="failures">The subscriber/exception pairs that failed during delivery.</param>
    /// <returns>A summary message describing the aggregated failures.</returns>
    private static string BuildMessage(ITopic topic, IReadOnlyList<(object Subscriber, Exception Exception)> failures)
    {
        var subscriberNames = failures.Select(f => f.Subscriber.GetType().Name);
        return $"{failures.Count} subscriber(s) failed while handling topic '{topic.DisplayName}': " +
               string.Join(", ", subscriberNames);
    }
}