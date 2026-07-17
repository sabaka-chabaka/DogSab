namespace DogSab.Platform.Core.Abstractions.Exceptions;

/// <summary>
/// Thrown when an internal platform invariant is violated
/// (e.g. a write operation called outside of a write action).
/// Analogous to AssertionError/PluginException in IntelliJ Platform.
/// </summary>
public sealed class PlatformInvariantViolationException : Exception
{
    /// <summary>
    /// Creates a new exception describing the violated invariant.
    /// </summary>
    /// <param name="message">A message describing which invariant was violated.</param>
    public PlatformInvariantViolationException(string message) : base(message)
    {
    }

    /// <summary>
    /// Creates a new exception describing the violated invariant, wrapping an underlying cause.
    /// </summary>
    /// <param name="message">A message describing which invariant was violated.</param>
    /// <param name="inner">The underlying exception that triggered the violation.</param>
    public PlatformInvariantViolationException(string message, Exception inner) : base(message, inner)
    {
    }
}