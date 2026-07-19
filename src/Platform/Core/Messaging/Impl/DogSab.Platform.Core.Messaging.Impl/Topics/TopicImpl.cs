using DogSab.Platform.Core.Abstractions.Messaging;

namespace DogSab.Platform.Core.Messaging.Impl.Topics;

/// <summary>
/// Default implementation of <see cref="ITopic{TListener}"/>.
/// A topic is a lightweight, immutable identity used purely as a lookup key
/// for subscriptions and publishing — it carries no behavior of its own.
/// Two <see cref="TopicImpl{TListener}"/> instances are distinct topics even if
/// they share the same <see cref="DisplayName"/>; identity is by reference,
/// matching how platform code is expected to declare one static topic instance
/// per event type and share it via a public static field.
/// </summary>
/// <typeparam name="TListener">The listener interface whose method calls represent messages on this topic.</typeparam>
public sealed class TopicImpl<TListener> : ITopic<TListener> where TListener : class
{
    /// <inheritdoc />
    public string DisplayName { get; }

    /// <summary>
    /// Creates a new topic identity.
    /// </summary>
    /// <param name="displayName">A human-readable name for logging and diagnostics.</param>
    public TopicImpl(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("Topic display name must not be null or empty.", nameof(displayName));
        }

        DisplayName = displayName;
    }
    
    /// <summary>
    /// Convenience factory, mirroring the common declaration pattern:
    /// <c>public static readonly ITopic&lt;IFooListener&gt; FOO = Topic.Create&lt;IFooListener&gt;("Foo");</c>
    /// </summary>
    /// <param name="displayName">A human-readable name for logging and diagnostics.</param>
    /// <returns>A new topic instance.</returns>
    public static ITopic<TListener> Create(string displayName) => new TopicImpl<TListener>(displayName);
}