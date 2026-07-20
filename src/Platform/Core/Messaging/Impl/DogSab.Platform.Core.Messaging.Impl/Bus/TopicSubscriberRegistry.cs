using System.Collections.Concurrent;
using DogSab.Platform.Core.Abstractions.Messaging;

namespace DogSab.Platform.Core.Messaging.Impl.Bus;

/// <summary>
/// Internal storage mapping each topic to its subscribed listener instances.
/// Subscribers are held via <see cref="WeakReference{T}"/> so a plugin that
/// forgets to dispose its <see cref="IMessageBusConnection"/> does not keep its
/// listener object alive indefinitely — the registry self-cleans dead entries
/// as it is accessed. Explicit disposal of the connection remains the primary,
/// deterministic way to unsubscribe; the weak-reference behavior is a safety
/// net, not a substitute for it.
/// </summary>
public sealed class TopicSubscriberRegistry
{
    /// <summary>Live subscriber lists per topic, keyed by topic identity (reference equality).</summary>
    private readonly ConcurrentDictionary<ITopic, List<WeakReference<object>>> _subscribersByTopic =
        new(ReferenceEqualityComparer.Instance);

    /// <summary>Guards mutation of an individual topic's subscriber list, since List&lt;T&gt; itself is not thread-safe.</summary>
    private readonly ConcurrentDictionary<ITopic, object> _topicLocks = new(ReferenceEqualityComparer.Instance);

    /// <summary>
    /// Registers a listener instance as a subscriber of the given topic.
    /// </summary>
    /// <param name="topic">The topic to subscribe to.</param>
    /// <param name="listener">The listener implementation to register.</param>
    public void Subscribe(ITopic topic, object listener)
    {
        var list = _subscribersByTopic.GetOrAdd(topic, static _ => new List<WeakReference<object>>());
        var lockObj = _topicLocks.GetOrAdd(topic, static _ => new object());

        lock (lockObj)
        {
            list.Add(new WeakReference<object>(listener));
        }
    }

    /// <summary>
    /// Removes a specific listener instance from a topic's subscriber list.
    /// Called when an <see cref="IMessageBusConnection"/> is disposed.
    /// </summary>
    /// <param name="topic">The topic to unsubscribe from.</param>
    /// <param name="listener">The listener instance to remove.</param>
    public void Unsubscribe(ITopic topic, object listener)
    {
        if (!_subscribersByTopic.TryGetValue(topic, out var list))
        {
            return;
        }

        var lockObj = _topicLocks.GetOrAdd(topic, static _ => new object());

        lock (lockObj)
        {
            list.RemoveAll(weakRef => !weakRef.TryGetTarget(out var target) || ReferenceEquals(target, listener));
        }
    }

    /// <summary>
    /// Returns a snapshot of all currently live subscriber instances for a topic,
    /// silently dropping and cleaning up any entries whose target has already
    /// been collected by the garbage collector.
    /// </summary>
    /// <param name="topic">The topic to look up subscribers for.</param>
    /// <returns>A snapshot list of live subscriber instances. Empty if the topic has no subscribers.</returns>
    public IReadOnlyList<object> GetLiveSubscribers(ITopic topic)
    {
        if (!_subscribersByTopic.TryGetValue(topic, out var list))
        {
            return Array.Empty<object>();
        }

        var lockObj = _topicLocks.GetOrAdd(topic, static _ => new object());

        lock (lockObj)
        {
            var live = new List<object>(list.Count);
            var deadCount = 0;

            foreach (var weakRef in list)
            {
                if (weakRef.TryGetTarget(out var target))
                {
                    live.Add(target);
                }
                else
                {
                    deadCount++;
                }
            }

            if (deadCount > 0)
            {
                list.RemoveAll(weakRef => !weakRef.TryGetTarget(out _));
            }

            return live;
        }
    }

    /// <summary>
    /// Removes every registered listener that originated from a specific connection.
    /// Used when an <see cref="IMessageBusConnection"/> is disposed with multiple
    /// active subscriptions across different topics.
    /// </summary>
    /// <param name="listener">The listener instance to remove from every topic it was subscribed to.</param>
    public void UnsubscribeFromAllTopics(object listener)
    {
        foreach (var topic in _subscribersByTopic.Keys.ToArray())
        {
            Unsubscribe(topic, listener);
        }
    }
    
    /// <summary>
    /// Returns a snapshot of every topic this registry currently knows about
    /// (i.e. that has had at least one subscriber at some point), together with
    /// its current live subscriber count. Used for startup/runtime diagnostics only.
    /// </summary>
    /// <returns>A snapshot list of (topic, live subscriber count) pairs.</returns>
    public IReadOnlyList<(ITopic Topic, int LiveSubscriberCount)> GetDiagnosticsSnapshot()
    {
        var result = new List<(ITopic, int)>();

        foreach (var topic in _subscribersByTopic.Keys)
        {
            result.Add((topic, GetLiveSubscribers(topic).Count));
        }

        return result;
    }
}