using System.Reflection;
using DogSab.Platform.Core.Abstractions.Messaging;
using DogSab.Platform.Core.Messaging.Impl.Diagnostics;

namespace DogSab.Platform.Core.Messaging.Impl.Delivery;

/// <summary>
/// Decides on which thread a published message is actually delivered to its
/// subscribers. Different topics have different needs: most platform events
/// (file changed, index updated) are fine being delivered synchronously on
/// whatever thread published them, while topics that directly touch UI state
/// must be marshalled onto the UI thread regardless of where they were published from.
/// </summary>
internal interface IMessageDeliveryStrategy
{
    /// <summary>
    /// Delivers a single published method call to every given live subscriber,
    /// recording any subscriber failures through <paramref name="exceptionPolicy"/>
    /// without letting one subscriber's failure prevent delivery to the others.
    /// </summary>
    /// <param name="topic">The topic being published to, used for diagnostics.</param>
    /// <param name="subscribers">The live subscriber instances to deliver to.</param>
    /// <param name="method">The listener interface method that was invoked by the publisher.</param>
    /// <param name="args">The arguments the publisher passed to <paramref name="method"/>.</param>
    /// <param name="exceptionPolicy">Policy used to record and optionally rethrow subscriber failures.</param>
    void Deliver(
        ITopic topic,
        IReadOnlyList<object> subscribers,
        MethodInfo method,
        object?[]? args,
        SubscriberExceptionPolicy exceptionPolicy);
}