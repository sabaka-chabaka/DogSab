namespace DogSab.Platform.Core.Abstractions.Exceptions;

/// <summary>Thrown when a service could not be resolved or constructed by the container.</summary>
public sealed class ServiceResolutionException : Exception
{
    /// <summary>The service type that failed to resolve.</summary>
    public Type ServiceType { get; }

    /// <summary>
    /// Creates a new exception describing a service resolution failure.
    /// </summary>
    /// <param name="serviceType">The service type that failed to resolve.</param>
    /// <param name="message">A message describing the cause of the failure.</param>
    /// <param name="inner">The underlying exception, if any.</param>
    public ServiceResolutionException(Type serviceType, string message, Exception? inner = null)
        : base(message, inner)
    {
        ServiceType = serviceType;
    }
}