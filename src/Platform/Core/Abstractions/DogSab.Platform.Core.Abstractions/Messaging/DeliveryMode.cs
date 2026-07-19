namespace DogSab.Platform.Core.Abstractions.Messaging;

/// <summary>
/// Determines on which thread messages published to a topic are delivered
/// to its subscribers. Fixed per topic at creation time, so every subscriber
/// of a given topic observes the same delivery behavior.
/// </summary>
public enum DeliveryMode
{
    /// <summary>
    /// Subscribers are invoked synchronously, on whatever thread the message
    /// was published from. The default for most platform events.
    /// </summary>
    Synchronous,

    /// <summary>
    /// Subscribers are always invoked on the UI thread, regardless of which
    /// thread published the message. Use for topics whose subscribers touch
    /// UI state directly (e.g. progress updates, editor markers).
    /// </summary>
    UiThread
}