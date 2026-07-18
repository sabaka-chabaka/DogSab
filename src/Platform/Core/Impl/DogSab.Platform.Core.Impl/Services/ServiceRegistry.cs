using System.Collections.Concurrent;
using DogSab.Platform.Core.Abstractions.Services;

namespace DogSab.Platform.Core.Impl.Services;


/// <summary>
/// Internal storage for service registrations, keyed by the public service interface type.
/// Does not create or hold instances — that responsibility belongs to <see cref="ServiceActivator"/>
/// and the scope-specific caches in <see cref="ServiceScopeImpl"/>.
/// </summary>
public sealed class ServiceRegistry
{
    /// <summary>All known registrations, keyed by their public service interface type.</summary>
    private readonly ConcurrentDictionary<Type, ServiceRegistration> _registrations = new();

    /// <summary>
    /// Adds or replaces the registration for a service interface.
    /// </summary>
    /// <param name="registration">The registration descriptor to store.</param>
    public void Add(ServiceRegistration registration)
    {
        _registrations[registration.ServiceType] = registration;
    }
    
    /// <summary>
    /// Attempts to find the registration for a given service interface type.
    /// </summary>
    /// <param name="serviceType">The service interface type to look up.</param>
    /// <param name="registration">The found registration, if any.</param>
    /// <returns><c>true</c> if a registration exists; otherwise <c>false</c>.</returns>
    public bool TryGet(Type serviceType, out ServiceRegistration registration)
    {
        return _registrations.TryGetValue(serviceType, out registration);
    }

    /// <summary>
    /// Checks whether a registration exists for the given service interface type.
    /// </summary>
    /// <param name="serviceType">The service interface type to check.</param>
    /// <returns><c>true</c> if a registration exists; otherwise <c>false</c>.</returns>
    public bool Contains(Type serviceType)
    {
        return _registrations.ContainsKey(serviceType);
    }
}