using DogSab.Platform.Core.Abstractions.Messaging;

namespace DogSab.Platform.Core.Messaging.Impl.Bus;

/// <summary>
/// Default implementation of <see cref="IMessageBusConnection"/>.
/// Tracks every listener subscribed through this connection so that disposing
/// it deterministically unsubscribes all of them at once, regardless of how
/// many different topics were subscribed to. This is the primary, explicit way
/// to unsubscribe; the registry's weak-reference cleanup is only a safety net
/// for listeners that were never properly unsubscribed.
/// </summary>
internal sealed class MessageBusConnectionImpl : IMessageBusConnection
{
    /// <summary>The registry this connection registers and unregisters listeners with.</summary>
    private readonly TopicSubscriberRegistry _subscriberRegistry;

    /// <summary>Every (topic, listener) pair subscribed through this connection, in subscription order.</summary>
    private readonly List<(ITopic Topic, object Listener)> _subscriptions = new();

    /// <summary>Guards <see cref="_subscriptions"/> and <see cref="_isDisposed"/> against concurrent access.</summary>
    private readonly object _lock = new();

    /// <summary>Whether this connection has already been disposed.</summary>
    private bool _isDisposed;

    /// <summary>
    /// Creates a new connection bound to a subscriber registry.
    /// </summary>
    /// <param name="subscriberRegistry">The registry this connection will register listeners with.</param>
    public MessageBusConnectionImpl(TopicSubscriberRegistry subscriberRegistry)
    {
        _subscriberRegistry = subscriberRegistry;
    }

    /// <summary>
    /// Subscribes a handler to a topic and records the subscription so it is
    /// automatically removed when this connection is disposed.
    /// </summary>
    /// <typeparam name="TListener">The listener interface associated with the topic.</typeparam>
    /// <param name="topic">The topic to subscribe to.</param>
    /// <param name="handler">The implementation invoked when a message is published to the topic.</param>
    /// <exception cref="ObjectDisposedException">Thrown if this connection has already been disposed.</exception>
    public void Subscribe<TListener>(ITopic<TListener> topic, TListener handler) where TListener : class
    {
        lock (_lock)
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(MessageBusConnectionImpl));
            }

            _subscriberRegistry.Subscribe(topic, handler);
            _subscriptions.Add((topic, handler));
        }
    }

    /// <summary>
    /// Unsubscribes every listener registered through this connection. Safe to
    /// call multiple times; subsequent calls are no-ops.
    /// </summary>
    public void Dispose()
    {
        List<(ITopic Topic, object Listener)> toRemove;

        lock (_lock)
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            toRemove = new List<(ITopic Topic, object Listener)>(_subscriptions);
            _subscriptions.Clear();
        }

        foreach (var (topic, listener) in toRemove)
        {
            _subscriberRegistry.Unsubscribe(topic, listener);
        }
    }
}