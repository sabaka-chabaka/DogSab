using DogSab.Platform.Core.Abstractions.Exceptions;
using DogSab.Platform.Core.Abstractions.Services;

namespace DogSab.Platform.Core.Impl.Services;

/// <summary>
/// Default implementation of <see cref="IServiceContainer"/>.
/// Resolves services lazily: a registration only produces an instance on first request,
/// after which singleton/scoped instances are cached according to their declared lifetime.
/// </summary>
public sealed class ServiceContainerImpl : IServiceContainer
{
    /// <summary>The scope this container instance serves (application- or project-level).</summary>
    private readonly ServiceScope _scope;

    /// <summary>Registrations known to this container.</summary>
    private readonly ServiceRegistry _registry;

    /// <summary>Cache of already-created singleton/scoped instances for this container's scope.</summary>
    private readonly ServiceScopeImpl _scopeCache;

    /// <summary>Creates service instances via constructor injection.</summary>
    private readonly ServiceActivator _activator;

    /// <summary>
    /// Creates a new service container for a given scope.
    /// </summary>
    /// <param name="scope">The scope this container serves (application- or project-level).</param>
    /// <param name="registry">The registry of known service registrations.</param>
    /// <param name="activator">The activator used to construct service instances.</param>
    public ServiceContainerImpl(ServiceScope scope, ServiceRegistry registry, ServiceActivator activator)
    {
        _scope = scope;
        _registry = registry;
        _scopeCache = new ServiceScopeImpl(scope);
        _activator = activator;
    }

    /// <summary>
    /// Registers a service implementation with this container.
    /// </summary>
    /// <param name="registration">The registration descriptor to add.</param>
    public void RegisterService(ServiceRegistration registration)
    {
        _registry.Add(registration);
    }

    /// <summary>
    /// Checks whether a service interface has a registered implementation.
    /// </summary>
    /// <typeparam name="T">The service interface to check.</typeparam>
    /// <returns><c>true</c> if a registration exists; otherwise <c>false</c>.</returns>
    public bool IsRegistered<T>() where T : class, IService
    {
        return _registry.Contains(typeof(T));
    }

    /// <summary>
    /// Resolves a service instance by its interface type, creating it lazily if needed.
    /// </summary>
    /// <typeparam name="T">The service interface to resolve.</typeparam>
    /// <returns>The resolved service instance.</returns>
    public T GetService<T>() where T : class, IService
    {
        return (T)GetService(typeof(T));
    }

    /// <summary>
    /// Attempts to resolve a service instance without throwing if it is not registered.
    /// </summary>
    /// <typeparam name="T">The service interface to resolve.</typeparam>
    /// <param name="service">The resolved instance, or <c>null</c> if not registered.</param>
    /// <returns><c>true</c> if the service was found; otherwise <c>false</c>.</returns>
    public bool TryGetService<T>(out T? service) where T : class, IService
    {
        if (!_registry.Contains(typeof(T)))
        {
            service = null;
            return false;
        }

        service = (T)GetService(typeof(T));
        return true;
    }

    /// <summary>
    /// Resolves a service instance by its runtime type, creating it lazily if needed
    /// and caching it according to its declared <see cref="ServiceLifetime"/>.
    /// </summary>
    /// <param name="serviceType">The service interface type to resolve.</param>
    /// <returns>The resolved service instance.</returns>
    /// <exception cref="ServiceResolutionException">
    /// Thrown if no registration exists for <paramref name="serviceType"/>, or if
    /// the registration's declared scope does not match this container's scope.
    /// </exception>
    public object GetService(Type serviceType)
    {
        if (!_registry.TryGet(serviceType, out var registration))
        {
            throw new ServiceResolutionException(serviceType, $"No registration found for service '{serviceType.FullName}'.");
        }

        if (registration.Scope != _scope)
        {
            throw new ServiceResolutionException(
                serviceType,
                $"Service '{serviceType.FullName}' is registered for scope '{registration.Scope}', " +
                $"but was requested from a container scoped to '{_scope}'.");
        }

        if (registration.Lifetime == ServiceLifetime.Transient)
        {
            return _activator.CreateInstance(registration, this);
        }

        if (_scopeCache.TryGetCached(serviceType, out var cached))
        {
            return cached!;
        }

        var instance = _activator.CreateInstance(registration, this);
        _scopeCache.Cache(serviceType, instance);
        return instance;
    }

    /// <summary>
    /// Clears all cached singleton/scoped instances held by this container.
    /// Called when the owning scope (e.g. a closed project) is torn down.
    /// </summary>
    public void ClearCache()
    {
        _scopeCache.Clear();
    }
}