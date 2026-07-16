using System;

namespace DogSab.Platform.Core.Abstractions.Messaging;

/// <summary>A subscription scope on the message bus; disposing it unsubscribes all its handlers.</summary>
public interface IMessageBusConnection : IDisposable
{
    /// <summary>
    /// Subscribes a handler to a topic. Unsubscribing happens via disposing the connection.
    /// </summary>
    /// <typeparam name="TListener">The listener interface associated with the topic.</typeparam>
    /// <param name="topic">The topic to subscribe to.</param>
    /// <param name="handler">The implementation invoked when a message is published to the topic.</param>
    void Subscribe<TListener>(ITopic<TListener> topic, TListener handler) where TListener : class;
}