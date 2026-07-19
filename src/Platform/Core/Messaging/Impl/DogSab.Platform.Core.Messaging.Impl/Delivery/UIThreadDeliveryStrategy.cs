using System.Reflection;
using DogSab.Platform.Core.Abstractions.Messaging;
using DogSab.Platform.Core.Abstractions.Threading;
using DogSab.Platform.Core.Messaging.Impl.Diagnostics;

namespace DogSab.Platform.Core.Messaging.Impl.Delivery;

/// <summary>
/// Delivers a published message to all subscribers on the UI thread, regardless
/// of which thread it was published from. If the publishing call is already on
/// the UI thread, delivery happens immediately and synchronously (no unnecessary
/// dispatch round-trip); otherwise the call is marshalled via
/// <see cref="IUiThreadDispatcher"/> and blocks the publisher until delivery completes,
/// so publishing behaves consistently regardless of the calling thread.
/// </summary>
internal sealed class UiThreadDeliveryStrategy : IMessageDeliveryStrategy
{
    /// <summary>Dispatcher used to marshal delivery onto the UI thread when necessary.</summary>
    private readonly IUiThreadDispatcher _uiThreadDispatcher;

    /// <summary>The synchronous strategy reused for the actual per-subscriber invocation once on the UI thread.</summary>
    private readonly SynchronousDeliveryStrategy _synchronousDelivery = new();

    /// <summary>
    /// Creates a new UI-thread delivery strategy.
    /// </summary>
    /// <param name="uiThreadDispatcher">Dispatcher used to marshal delivery onto the UI thread.</param>
    public UiThreadDeliveryStrategy(IUiThreadDispatcher uiThreadDispatcher)
    {
        _uiThreadDispatcher = uiThreadDispatcher;
    }

    /// <inheritdoc />
    public void Deliver(
        ITopic topic,
        IReadOnlyList<object> subscribers,
        MethodInfo method,
        object?[]? args,
        SubscriberExceptionPolicy exceptionPolicy)
    {
        if (_uiThreadDispatcher.IsUiThread)
        {
            _synchronousDelivery.Deliver(topic, subscribers, method, args, exceptionPolicy);
            return;
        }

        Exception? capturedException = null;

        _uiThreadDispatcher.Invoke(() =>
        {
            try
            {
                _synchronousDelivery.Deliver(topic, subscribers, method, args, exceptionPolicy);
            }
            catch (Exception ex)
            {
                // Only reachable if exceptionPolicy is configured to rethrow
                // (MessageDeliveryException) — capture it to rethrow on the caller's
                // original thread instead of losing it inside the dispatched action.
                capturedException = ex;
            }
        });

        if (capturedException is not null)
        {
            throw capturedException;
        }
    }
}