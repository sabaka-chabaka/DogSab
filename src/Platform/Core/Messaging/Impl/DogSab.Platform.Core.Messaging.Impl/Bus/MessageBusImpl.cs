using DogSab.Platform.Core.Abstractions.Logging;
using DogSab.Platform.Core.Abstractions.Messaging;
using DogSab.Platform.Core.Abstractions.Threading;
using DogSab.Platform.Core.Messaging.Impl.Delivery;
using DogSab.Platform.Core.Messaging.Impl.Diagnostics;
using DogSab.Platform.Core.Messaging.Impl.Proxy;

namespace DogSab.Platform.Core.Messaging.Impl.Bus;

/// <summary>
/// Default implementation of <see cref="IMessageBus"/>.
/// Ties together the subscriber registry, delivery strategy resolution, and
/// publisher proxy caching into the platform's single publish/subscribe hub.
/// One instance of this type is shared for the entire application; both
/// application- and project-scoped code publish and subscribe through it.
/// </summary>
public sealed class MessageBusImpl : IMessageBus
{
    /// <summary>Storage of which listener instances are subscribed to which topics.</summary>
    private readonly TopicSubscriberRegistry _subscriberRegistry = new();

    /// <summary>Cache of publisher proxies, one per topic.</summary>
    private readonly PublisherProxyCache _proxyCache;

    /// <summary>
    /// Creates a new message bus.
    /// </summary>
    /// <param name="uiThreadDispatcher">Dispatcher used to deliver messages for UI-thread topics.</param>
    /// <param name="loggerFactory">Factory used to obtain loggers for delivery diagnostics.</param>
    /// <param name="rethrowSubscriberExceptions">
    /// Whether a failing subscriber's exception should be rethrown to the publisher
    /// after all subscribers have been attempted. Defaults to <c>false</c> — see
    /// <see cref="SubscriberExceptionPolicy"/> for the rationale.
    /// </param>
    public MessageBusImpl(
        IUiThreadDispatcher uiThreadDispatcher,
        ILoggerFactory loggerFactory,
        bool rethrowSubscriberExceptions = false)
    {
        var deliveryStrategyResolver = new DeliveryStrategyResolver(uiThreadDispatcher);
        var exceptionPolicy = new SubscriberExceptionPolicy(loggerFactory, rethrowSubscriberExceptions);

        _proxyCache = new PublisherProxyCache(_subscriberRegistry, deliveryStrategyResolver, exceptionPolicy);
    }

    /// <summary>
    /// Returns a proxy object of type <typeparamref name="TListener"/>: calling any
    /// of its methods broadcasts a message to all subscribers of the topic.
    /// </summary>
    /// <typeparam name="TListener">The listener interface associated with the topic.</typeparam>
    /// <param name="topic">The topic to publish to.</param>
    /// <returns>A cached proxy implementing <typeparamref name="TListener"/> that broadcasts calls to subscribers.</returns>
    public TListener Publisher<TListener>(ITopic<TListener> topic) where TListener : class
    {
        return _proxyCache.GetOrCreate(topic);
    }

    /// <summary>
    /// Opens a new subscription connection. Dispose the connection to unsubscribe
    /// all handlers registered through it.
    /// </summary>
    /// <returns>A new <see cref="IMessageBusConnection"/> bound to this bus's subscriber registry.</returns>
    public IMessageBusConnection Connect()
    {
        return new MessageBusConnectionImpl(_subscriberRegistry);
    }
}