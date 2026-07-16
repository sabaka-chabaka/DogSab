namespace DogSab.Platform.Core.Abstractions.Messaging;

/// <summary>Contract for the platform's publish/subscribe event bus.</summary>
public interface IMessageBus
{
    /// <summary>
    /// Returns a proxy object of type TListener: calling any of its methods
    /// broadcasts a message to all subscribers of the topic.
    /// </summary>
    /// <typeparam name="TListener">The listener interface associated with the topic.</typeparam>
    /// <param name="topic">The topic to publish to.</param>
    /// <returns>A proxy implementing <typeparamref name="TListener"/> that broadcasts calls to subscribers.</returns>
    TListener Publisher<TListener>(ITopic<TListener> topic) where TListener : class;

    /// <summary>
    /// Opens a new subscription connection. Dispose the connection to unsubscribe all its handlers.
    /// </summary>
    /// <returns>A new <see cref="IMessageBusConnection"/>.</returns>
    IMessageBusConnection Connect();
}