using DogSab.Platform.Core.Abstractions.Services;

namespace DogSab.Platform.Core.Impl.DependencyInjection;

/// <summary>
/// Exposes a platform <see cref="IServiceContainer"/> as a standard
/// <see cref="IServiceProvider"/>, so third-party libraries that expect
/// Microsoft.Extensions.DependencyInjection can be wired into the platform's
/// own DI without every plugin needing to know about <see cref="IServiceContainer"/>.
/// Only resolves types that implement <see cref="IService"/> and are registered
/// with the wrapped container; unrelated types simply fail to resolve, matching
/// standard <see cref="IServiceProvider"/> semantics.
/// </summary>
public sealed class MicrosoftDiAdapter : IServiceProvider
{
    /// <summary>The platform container this adapter delegates to.</summary>
    private readonly IServiceContainer _container;

    /// <summary>
    /// Creates a new adapter wrapping a platform service container.
    /// </summary>
    /// <param name="container">The platform container to expose as an <see cref="IServiceProvider"/>.</param>
    public MicrosoftDiAdapter(IServiceContainer container)
    {
        _container = container;
    }

    /// <summary>
    /// Resolves a service instance by its runtime type through the wrapped platform container.
    /// </summary>
    /// <param name="serviceType">The service type to resolve.</param>
    /// <returns>The resolved instance, or <c>null</c> if resolution fails.</returns>
    public object? GetService(Type serviceType)
    {
        if (!typeof(IService).IsAssignableFrom(serviceType))
        {
            return null;
        }

        return _container.TryGetServiceUntyped(serviceType, out var instance)
            ? instance
            : null;
    }
}