namespace DogSab.Platform.Core.Abstractions.Services;

/// <summary>Describes how a single service should be registered with the container.</summary>
public readonly struct ServiceRegistration
{
    /// <summary>The public interface type consumers will request.</summary>
    public Type ServiceType { get; }

    /// <summary>The concrete type that implements <see cref="ServiceType"/>.</summary>
    public Type ImplementationType { get; }

    /// <summary>The scope at which the instance is shared (application- or project-level).</summary>
    public ServiceScope Scope { get; }

    /// <summary>How instances are created and reused by the container.</summary>
    public ServiceLifetime Lifetime { get; }

    /// <summary>
    /// Creates a new service registration descriptor.
    /// </summary>
    /// <param name="serviceType">The public interface type consumers will request.</param>
    /// <param name="implementationType">The concrete type that implements <paramref name="serviceType"/>.</param>
    /// <param name="scope">The scope at which the instance is shared.</param>
    /// <param name="lifetime">How instances are created and reused.</param>
    public ServiceRegistration(
        Type serviceType,
        Type implementationType,
        ServiceScope scope,
        ServiceLifetime lifetime)
    {
        ServiceType = serviceType;
        ImplementationType = implementationType;
        Scope = scope;
        Lifetime = lifetime;
    }
}