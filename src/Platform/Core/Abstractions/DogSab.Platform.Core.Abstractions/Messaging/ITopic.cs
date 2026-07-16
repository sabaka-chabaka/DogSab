namespace DogSab.Platform.Core.Abstractions.Messaging;

/// <summary>Non-generic base for a topic, used for reflection/registry purproses.</summary>
public interface ITopic
{
    /// <summary>Human-readable name of the topic, used for logging and diagnostics.</summary>
    string DisplayName { get; }
}

/// <summary>
/// A type-safe message channel. TListener is an interface whose methods act as event handlers.
/// </summary>
/// <typeparam name="TListener">The listener interface whose method calls are broadcast as messages.</typeparam>
public interface ITopic<TListener> : ITopic where TListener : class
{
}