using System.Collections.Concurrent;
using System.Reflection;
using DogSab.Platform.Core.Abstractions.Messaging;
using DogSab.Platform.Core.Messaging.Impl.Bus;
using DogSab.Platform.Core.Messaging.Impl.Delivery;
using DogSab.Platform.Core.Messaging.Impl.Diagnostics;

namespace DogSab.Platform.Core.Messaging.Impl.Proxy;

/// <summary>
/// Caches one <see cref="MulticastPublisherProxy"/> instance per topic, since
/// constructing a <see cref="DispatchProxy"/> involves reflection-based type
/// generation on first use per listener interface and is unnecessary to repeat
/// on every call to <see cref="IMessageBus.Publisher{TListener}"/> for the same topic.
/// </summary>
internal sealed class PublisherProxyCache
{
    /// <summary>Cached proxy instances, keyed by topic identity (reference equality).</summary>
    private readonly ConcurrentDictionary<ITopic, object> _proxiesByTopic =
        new(ReferenceEqualityComparer.Instance);

    /// <summary>Registry passed to newly created proxies to look up live subscribers.</summary>
    private readonly TopicSubscriberRegistry _subscriberRegistry;

    /// <summary>Resolver passed to newly created proxies to pick a delivery strategy.</summary>
    private readonly DeliveryStrategyResolver _deliveryStrategyResolver;

    /// <summary>Exception policy passed to newly created proxies.</summary>
    private readonly SubscriberExceptionPolicy _exceptionPolicy;

    /// <summary>
    /// Creates a new proxy cache.
    /// </summary>
    /// <param name="subscriberRegistry">Registry used by created proxies to look up live subscribers.</param>
    /// <param name="deliveryStrategyResolver">Resolver used by created proxies to pick a delivery strategy.</param>
    /// <param name="exceptionPolicy">Exception policy used by created proxies.</param>
    public PublisherProxyCache(
        TopicSubscriberRegistry subscriberRegistry,
        DeliveryStrategyResolver deliveryStrategyResolver,
        SubscriberExceptionPolicy exceptionPolicy)
    {
        _subscriberRegistry = subscriberRegistry;
        _deliveryStrategyResolver = deliveryStrategyResolver;
        _exceptionPolicy = exceptionPolicy;
    }

    /// <summary>
    /// Returns the cached publisher proxy for a topic, creating and configuring
    /// a new one on first request.
    /// </summary>
    /// <typeparam name="TListener">The listener interface the returned proxy implements.</typeparam>
    /// <param name="topic">The topic to get a publisher proxy for.</param>
    /// <returns>A <typeparamref name="TListener"/> instance backed by a <see cref="MulticastPublisherProxy"/>.</returns>
    public TListener GetOrCreate<TListener>(ITopic<TListener> topic) where TListener : class
    {
        var proxyObject = _proxiesByTopic.GetOrAdd(topic, CreateProxy<TListener>);
        return (TListener)proxyObject;
    }

    /// <summary>
    /// Creates and configures a new <see cref="MulticastPublisherProxy"/> for a topic.
    /// Used as the factory delegate for <see cref="ConcurrentDictionary{TKey,TValue}.GetOrAdd(TKey,Func{TKey,TValue})"/>.
    /// </summary>
    /// <typeparam name="TListener">The listener interface the proxy should implement.</typeparam>
    /// <param name="topic">The topic the new proxy will publish to.</param>
    /// <returns>The newly created and configured proxy, boxed as <see cref="object"/> for cache storage.</returns>
    private object CreateProxy<TListener>(ITopic topic) where TListener : class
    {
        var proxy = DispatchProxy.Create<TListener, MulticastPublisherProxy>();
        ((MulticastPublisherProxy)(object)proxy).Configure(
            topic,
            _subscriberRegistry,
            _deliveryStrategyResolver,
            _exceptionPolicy);

        return proxy;
    }
}