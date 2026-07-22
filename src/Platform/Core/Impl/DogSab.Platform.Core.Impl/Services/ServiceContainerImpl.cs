using DogSab.Platform.Core.Abstractions.Exceptions;
using DogSab.Platform.Core.Abstractions.Services;

namespace DogSab.Platform.Core.Impl.Services;

/// <summary>
/// Default implementation of <see cref="IServiceContainer"/>.
/// Resolves services lazily: a registration only produces an instance on first request,
/// after which singleton/scoped instances are cached according to their declared lifetime.
/// Containers can be chained via <see cref="Parent"/>: a project-scoped container falls
/// back to its parent application-scoped container when a service is not registered locally.
/// </summary>
public class ServiceContainerImpl : IServiceContainer
{
    private readonly ServiceScope _scope;
    private readonly ServiceRegistry _registry;
    private readonly ServiceScopeImpl _scopeCache;
    private readonly ServiceActivator _activator;

    /// <summary>
    /// The parent container to delegate to when a service is not registered in this
    /// container's own scope, or <c>null</c> if this is the root (application) container.
    /// </summary>
    public IServiceContainer? Parent { get; }

    /// <summary>
    /// Creates a new root (application-level) service container without a parent.
    /// </summary>
    public ServiceContainerImpl(ServiceRegistry registry, ServiceActivator activator)
        : this(ServiceScope.Application, registry, activator, parent: null)
    {
    }

    /// <summary>
    /// Creates a new child (project-level) service container that delegates to a parent
    /// when a service is not registered in its own scope.
    /// </summary>
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
    public void RegisterService(ServiceRegistration registration)
    {
        _registry.Add(registration);
    }

    /// <summary>
    /// Registers an already-constructed instance as the implementation of a
    /// service interface, bypassing <see cref="ServiceActivator"/> entirely.
    /// Used by <c>PlatformBootstrapper</c> to register the core singletons it
    /// constructs by hand during the bootstrap sequence.
    /// </summary>
    /// <typeparam name="T">The service interface to register the instance under.</typeparam>
    /// <param name="instance">The already-constructed instance to serve for this service type.</param>
    public void RegisterInstance<T>(T instance) where T : notnull
    {
        _registry.Add(new ServiceRegistration(typeof(T), instance.GetType(), _scope, ServiceLifetime.Singleton));
        _scopeCache.Cache(typeof(T), instance);
    }

    /// <summary>
    /// Checks whether a service interface has a registered implementation,
    /// searching this container's own scope and then its parent chain.
    /// </summary>
    public bool IsRegistered<T>() where T : class, IService
    {
        if (_registry.Contains(typeof(T)))
        {
            return true;
        }

        return Parent?.IsRegistered<T>() ?? false;
    }

    /// <inheritdoc />
    public T GetService<T>() where T : class, IService
    {
        return (T)GetService(typeof(T));
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public virtual object GetService(Type serviceType)
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
    /// </summary>
    public virtual void ClearCache()
    {
        _scopeCache.Clear();
    }
}