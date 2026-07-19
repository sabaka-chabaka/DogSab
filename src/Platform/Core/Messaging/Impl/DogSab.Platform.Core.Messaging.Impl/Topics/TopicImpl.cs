using DogSab.Platform.Core.Abstractions.Messaging;

namespace DogSab.Platform.Core.Messaging.Impl.Topics;

/// <summary>
/// Default implementation of <see cref="ITopic{TListener}"/>.
/// A topic is a lightweight, immutable identity used purely as a lookup key
/// for subscriptions and publishing. Two <see cref="TopicImpl{TListener}"/>
/// instances are distinct topics even if they share the same
/// <see cref="DisplayName"/>; identity is by reference, matching how platform
/// code is expected to declare one static topic instance per event type and
/// share it via a public static field.
/// </summary>
/// <typeparam name="TListener">The listener interface whose method calls represent messages on this topic.</typeparam>
public sealed class TopicImpl<TListener> : ITopic<TListener> where TListener : class
{
    /// <inheritdoc />
    public string DisplayName { get; }

    /// <inheritdoc />
    public DeliveryMode DeliveryMode { get; }

    /// <summary>
    /// Creates a new topic identity.
    /// </summary>
    /// <param name="displayName">A human-readable name for logging and diagnostics.</param>
    /// <param name="deliveryMode">The thread on which subscribers are invoked. Defaults to <see cref="Abstractions.Messaging.DeliveryMode.Synchronous"/>.</param>
    public TopicImpl(string displayName, DeliveryMode deliveryMode = DeliveryMode.Synchronous)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("Topic display name must not be null or empty.", nameof(displayName));
        }

        DisplayName = displayName;
        DeliveryMode = deliveryMode;
    }

    /// <summary>
    /// Convenience factory, mirroring the common declaration pattern:
    /// <c>public static readonly ITopic&lt;IFooListener&gt; FOO = TopicImpl&lt;IFooListener&gt;.Create("Foo");</c>
    /// </summary>
    /// <param name="displayName">A human-readable name for logging and diagnostics.</param>
    /// <param name="deliveryMode">The thread on which subscribers are invoked. Defaults to synchronous delivery.</param>
    /// <returns>A new topic instance.</returns>
    public static ITopic<TListener> Create(string displayName, DeliveryMode deliveryMode = DeliveryMode.Synchronous)
        => new TopicImpl<TListener>(displayName, deliveryMode);
}