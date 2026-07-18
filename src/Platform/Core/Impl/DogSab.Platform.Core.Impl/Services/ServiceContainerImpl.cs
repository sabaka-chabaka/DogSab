using DogSab.Platform.Core.Abstractions.Exceptions;
using DogSab.Platform.Core.Abstractions.Services;

namespace DogSab.Platform.Core.Impl.Services;

/// <summary>
/// Default implementation of <see cref="IServiceContainer"/>.
/// Resolves services lazily: a registration only produces an instance on first request,
/// after which singleton/scoped instances are cached according to their declared lifetime.
/// Containers can be chained via <see cref="Parent"/>: a project-scoped container falls
/// back to its parent application-scoped container when a service is not registered locally,
/// mirroring how a project can transparently access application-level services.
/// </summary>
public sealed class ServiceContainerImpl : IServiceContainer
{
    /// <summary>The scope this container instance serves (application- or project-level).</summary>
    private readonly ServiceScope _scope;

    /// <summary>Registrations known to this container's own scope.</summary>
    private readonly ServiceRegistry _registry;

    /// <summary>Cache of already-created singleton/scoped instances for this container's own scope.</summary>
    private readonly ServiceScopeImpl _scopeCache;

    /// <summary>Creates service instances via constructor injection.</summary>
    private readonly ServiceActivator _activator;

    /// <summary>
    /// The parent container to delegate to when a service is not registered in this
    /// container's own scope, or <c>null</c> if this is the root (application) container.
    /// </summary>
    public IServiceContainer? Parent { get; }

    /// <summary>
    /// Creates a new root (application-level) service container without a parent.
    /// </summary>
    /// <param name="registry">The registry of known service registrations for this scope.</param>
    /// <param name="activator">The activator used to construct service instances.</param>
    public ServiceContainerImpl(ServiceRegistry registry, ServiceActivator activator)
        : this(ServiceScope.Application, registry, activator, parent: null)
    {
    }

    /// <summary>
    /// Creates a new child (project-level) service container that delegates to a parent
    /// when a service is not registered in its own scope.
    /// </summary>
    /// <param name="scope">The scope this container serves.</param>
    /// <param name="registry">The registry of known service registrations for this scope.</param>
    /// <param name="activator">The activator used to construct service instances.</param>
    /// <param name="parent">The parent container to fall back to, or <c>null</c> for the root container.</param>
    public ServiceContainerImpl(ServiceScope scope, ServiceRegistry registry, ServiceActivator activator, IServiceContainer? parent)
    {
        _scope = scope;
        _registry = registry;
        _scopeCache = new ServiceScopeImpl(scope);
        _activator = activator;
        Parent = parent;
    }

    /// <summary>
    /// Registers a service implementation with this container's own scope.
    /// </summary>
    /// <param name="registration">The registration descriptor to add.</param>
    public void RegisterService(ServiceRegistration registration)
    {
        _registry.Add(registration);
    }

    /// <summary>
    /// Checks whether a service interface has a registered implementation,
    /// searching this container's own scope and then its parent chain.
    /// </summary>
    /// <typeparam name="T">The service interface to check.</typeparam>
    /// <returns><c>true</c> if a registration exists locally or in a parent container; otherwise <c>false</c>.</returns>
    public bool IsRegistered<T>() where T : class, IService
    {
        if (_registry.Contains(typeof(T)))
        {
            return true;
        }

        return Parent?.IsRegistered<T>() ?? false;
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
    /// Attempts to resolve a service instance without throwing if it is not registered
    /// anywhere in this container's own scope or its parent chain.
    /// </summary>
    /// <typeparam name="T">The service interface to resolve.</typeparam>
    /// <param name="service">The resolved instance, or <c>null</c> if not registered.</param>
    /// <returns><c>true</c> if the service was found; otherwise <c>false</c>.</returns>
    public bool TryGetService<T>(out T? service) where T : class, IService
    {
        if (!IsRegistered<T>())
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
    /// If the service is not registered in this container's own scope, the request
    /// is delegated to <see cref="Parent"/>, if one exists.
    /// </summary>
    /// <param name="serviceType">The service interface type to resolve.</param>
    /// <returns>The resolved service instance.</returns>
    /// <exception cref="ServiceResolutionException">
    /// Thrown if no registration exists for <paramref name="serviceType"/> in this
    /// container's own scope or anywhere in its parent chain.
    /// </exception>
    public object GetService(Type serviceType)
    {
        if (_registry.TryGet(serviceType, out var registration))
        {
            return ResolveLocal(serviceType, registration);
        }

        if (Parent is not null)
        {
            return Parent.GetService(serviceType);
        }

        throw new ServiceResolutionException(
            serviceType,
            $"No registration found for service '{serviceType.FullName}' in scope '{_scope}' or any parent scope.");
    }

    /// <summary>
    /// Resolves a service registration that was found in this container's own registry,
    /// applying its lifetime and populating the local instance cache as needed.
    /// </summary>
    /// <param name="serviceType">The service type being resolved.</param>
    /// <param name="registration">The local registration describing how to construct the service.</param>
    /// <returns>The resolved service instance.</returns>
    private object ResolveLocal(Type serviceType, ServiceRegistration registration)
    {
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
    /// Clears all cached singleton/scoped instances held by this container's own scope.
    /// Does not affect the parent container's cache. Called when the owning scope
    /// (e.g. a closed project) is torn down.
    /// </summary>
    public void ClearCache()
    {
        _scopeCache.Clear();
    }
}