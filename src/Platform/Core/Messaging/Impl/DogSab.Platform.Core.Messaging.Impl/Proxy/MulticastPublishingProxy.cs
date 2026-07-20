using System.Reflection;
using DogSab.Platform.Core.Abstractions.Messaging;
using DogSab.Platform.Core.Messaging.Impl.Bus;
using DogSab.Platform.Core.Messaging.Impl.Delivery;
using DogSab.Platform.Core.Messaging.Impl.Diagnostics;

namespace DogSab.Platform.Core.Messaging.Impl.Proxy;

/// <summary>
/// A <see cref="DispatchProxy"/>-based implementation of a topic's listener
/// interface: calling any method on an instance of this proxy does not run
/// real logic itself, but instead looks up every live subscriber of the
/// associated topic and re-invokes the same method with the same arguments
/// on each of them, via the topic's configured <see cref="IMessageDeliveryStrategy"/>.
/// This is the mechanism behind <see cref="IMessageBus.Publisher{TListener}"/>:
/// the object it returns is an instance of this proxy, cast to <c>TListener</c>.
/// </summary>
/// <remarks>
/// <see cref="DispatchProxy"/> requires a public parameterless constructor and
/// mutable public fields to receive its configuration after construction, since
/// <see cref="DispatchProxy.Create{T,TProxy}"/> does not accept constructor
/// arguments. Configuration is therefore injected via <see cref="Configure"/>
/// immediately after creation, before the proxy is ever handed to calling code.
/// </remarks>
public sealed class MulticastPublisherProxy : DispatchProxy
{
    /// <summary>The topic this proxy publishes to. Set via <see cref="Configure"/>.</summary>
    private ITopic _topic = null!;

    /// <summary>Registry used to look up the topic's live subscribers. Set via <see cref="Configure"/>.</summary>
    private TopicSubscriberRegistry _subscriberRegistry = null!;

    /// <summary>Resolver used to pick the delivery strategy matching the topic's delivery mode. Set via <see cref="Configure"/>.</summary>
    private DeliveryStrategyResolver _deliveryStrategyResolver = null!;

    /// <summary>Policy governing how subscriber exceptions are recorded and optionally rethrown. Set via <see cref="Configure"/>.</summary>
    private SubscriberExceptionPolicy _exceptionPolicy = null!;

    /// <summary>
    /// Injects this proxy's dependencies after construction. Must be called exactly
    /// once, immediately after <see cref="DispatchProxy.Create{T,TProxy}"/>, before
    /// the proxy is exposed to any caller as a <c>TListener</c>.
    /// </summary>
    /// <param name="topic">The topic this proxy publishes to.</param>
    /// <param name="subscriberRegistry">Registry used to look up the topic's live subscribers.</param>
    /// <param name="deliveryStrategyResolver">Resolver used to pick the delivery strategy for the topic.</param>
    /// <param name="exceptionPolicy">Policy governing subscriber exception handling.</param>
    internal void Configure(
        ITopic topic,
        TopicSubscriberRegistry subscriberRegistry,
        DeliveryStrategyResolver deliveryStrategyResolver,
        SubscriberExceptionPolicy exceptionPolicy)
    {
        _topic = topic;
        _subscriberRegistry = subscriberRegistry;
        _deliveryStrategyResolver = deliveryStrategyResolver;
        _exceptionPolicy = exceptionPolicy;
    }

    /// <summary>
    /// Intercepts every call made through the proxy's <c>TListener</c> interface
    /// and fans it out to every live subscriber of the configured topic.
    /// </summary>
    /// <param name="targetMethod">The listener interface method the caller invoked.</param>
    /// <param name="args">The arguments the caller passed.</param>
    /// <returns><c>null</c> always — listener interface methods are expected to return <c>void</c>.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if <paramref name="targetMethod"/> is <c>null</c>, which should not
    /// occur in practice for a correctly generated <see cref="DispatchProxy"/>.
    /// </exception>
    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        if (targetMethod is null)
        {
            throw new InvalidOperationException("DispatchProxy invoked with a null target method.");
        }

        if (targetMethod.ReturnType != typeof(void))
        {
            throw new InvalidOperationException(
                $"Topic listener method '{targetMethod.Name}' must return void. " +
                $"Message bus listener interfaces are event handlers and cannot return values.");
        }

        var subscribers = _subscriberRegistry.GetLiveSubscribers(_topic);

        if (subscribers.Count == 0)
        {
            return null;
        }

        var strategy = _deliveryStrategyResolver.Resolve(_topic);
        strategy.Deliver(_topic, subscribers, targetMethod, args, _exceptionPolicy);

        return null;
    }
}