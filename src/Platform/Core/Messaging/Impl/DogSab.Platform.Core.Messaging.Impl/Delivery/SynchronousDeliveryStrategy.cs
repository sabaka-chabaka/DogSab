using System.Reflection;
using DogSab.Platform.Core.Abstractions.Messaging;
using DogSab.Platform.Core.Messaging.Impl.Diagnostics;

namespace DogSab.Platform.Core.Messaging.Impl.Delivery;

/// <summary>
/// Delivers a published message to all subscribers synchronously, on the same
/// thread that published it. This is the default strategy for most platform
/// topics: the publishing call blocks until every subscriber has been invoked.
/// </summary>
internal sealed class SynchronousDeliveryStrategy : IMessageDeliveryStrategy
{
    /// <inheritdoc />
    public void Deliver(
        ITopic topic,
        IReadOnlyList<object> subscribers,
        MethodInfo method,
        object?[]? args,
        SubscriberExceptionPolicy exceptionPolicy)
    {
        var failures = new List<(object Subscriber, Exception Exception)>();

        foreach (var subscriber in subscribers)
        {
            try
            {
                method.Invoke(subscriber, args);
            }
            catch (TargetInvocationException ex) when (ex.InnerException is not null)
            {
                // Reflection wraps the subscriber's real exception; unwrap it so
                // logs and MessageDeliveryException report the actual failure cause.
                exceptionPolicy.RecordFailure(topic, subscriber, ex.InnerException);
                failures.Add((subscriber, ex.InnerException));
            }
            catch (Exception ex)
            {
                exceptionPolicy.RecordFailure(topic, subscriber, ex);
                failures.Add((subscriber, ex));
            }
        }

        exceptionPolicy.HandleCollectedFailures(topic, failures);
    }
}