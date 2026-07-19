using DogSab.Platform.Core.Abstractions.Messaging;
using DogSab.Platform.Core.Abstractions.Threading;

namespace DogSab.Platform.Core.Messaging.Impl.Delivery;

/// <summary>
/// Selects the <see cref="IMessageDeliveryStrategy"/> matching a topic's declared
/// <see cref="ITopic.DeliveryMode"/>. Strategy instances are stateless and shared,
/// so this resolver caches one instance per mode rather than constructing a new
/// strategy on every publish call.
/// </summary>
internal sealed class DeliveryStrategyResolver
{
    /// <summary>The strategy used for topics with <see cref="DeliveryMode.Synchronous"/>.</summary>
    private readonly SynchronousDeliveryStrategy _synchronousStrategy = new();

    /// <summary>The strategy used for topics with <see cref="DeliveryMode.UiThread"/>.</summary>
    private readonly UiThreadDeliveryStrategy _uiThreadStrategy;

    /// <summary>
    /// Creates a new resolver.
    /// </summary>
    /// <param name="uiThreadDispatcher">Dispatcher passed to the UI-thread delivery strategy.</param>
    public DeliveryStrategyResolver(IUiThreadDispatcher uiThreadDispatcher)
    {
        _uiThreadStrategy = new UiThreadDeliveryStrategy(uiThreadDispatcher);
    }

    /// <summary>
    /// Returns the delivery strategy matching a topic's declared delivery mode.
    /// </summary>
    /// <param name="topic">The topic whose delivery mode determines the strategy.</param>
    /// <returns>The strategy instance to use for delivering messages on this topic.</returns>
    public IMessageDeliveryStrategy Resolve(ITopic topic)
    {
        return topic.DeliveryMode switch
        {
            DeliveryMode.Synchronous => _synchronousStrategy,
            DeliveryMode.UiThread => _uiThreadStrategy,
            _ => throw new ArgumentOutOfRangeException(
                nameof(topic),
                topic.DeliveryMode,
                $"Unhandled delivery mode for topic '{topic.DisplayName}'.")
        };
    }
}