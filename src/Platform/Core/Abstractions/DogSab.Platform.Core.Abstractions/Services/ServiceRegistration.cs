namespace DogSab.Platform.Core.Abstractions.Services;

public readonly struct ServiceRegistration
{
    public Type ServiceType { get; }
    public Type ImplementationType { get; }
    public ServiceScope Scope { get; }
    public ServiceLifetime Lifetime { get; }

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