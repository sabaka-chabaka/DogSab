using DogSab.Platform.Core.Abstractions.Lifecycle;

namespace DogSab.Platform.Core.Impl.Services;

using System;
using System.Collections.Concurrent;
using DogSab.Platform.Core.Abstractions.Services;

/// <summary>
/// Holds cached singleton/scoped service instances for one scope
/// (either the single application scope, or one scope per open project).
/// </summary>
internal sealed class ServiceScopeImpl
{
    /// <summary>The scope this instance cache belongs to.</summary>
    public ServiceScope Scope { get; }

    /// <summary>Cached instances for services with <see cref="ServiceLifetime.Singleton"/> or <see cref="ServiceLifetime.Scoped"/> lifetime, keyed by service type.</summary>
    private readonly ConcurrentDictionary<Type, object> _cachedInstances = new();

    /// <summary>
    /// Creates a new instance cache for a scope.
    /// </summary>
    /// <param name="scope">The scope this cache belongs to.</param>
    public ServiceScopeImpl(ServiceScope scope)
    {
        Scope = scope;
    }

    /// <summary>
    /// Attempts to retrieve a previously cached instance for the given service type.
    /// </summary>
    /// <param name="serviceType">The service type to look up.</param>
    /// <param name="instance">The cached instance, if present.</param>
    /// <returns><c>true</c> if an instance was already cached; otherwise <c>false</c>.</returns>
    public bool TryGetCached(Type serviceType, out object? instance)
    {
        return _cachedInstances.TryGetValue(serviceType, out instance);
    }

    /// <summary>
    /// Stores an instance in the cache for the given service type.
    /// </summary>
    /// <param name="serviceType">The service type the instance was created for.</param>
    /// <param name="instance">The instance to cache.</param>
    public void Cache(Type serviceType, object instance)
    {
        _cachedInstances[serviceType] = instance;
    }

    /// <summary>
    /// Removes all cached instances from this scope. Called when the owning
    /// scope (e.g. a project) is being disposed. Does not call Dispose on the
    /// instances themselves — that is the responsibility of <see cref="IDisposableRegistry"/>.
    /// </summary>
    public void Clear()
    {
        _cachedInstances.Clear();
    }
}