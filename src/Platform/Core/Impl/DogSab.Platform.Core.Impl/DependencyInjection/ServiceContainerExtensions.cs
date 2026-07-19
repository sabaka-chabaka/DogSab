using DogSab.Platform.Core.Abstractions.Exceptions;
using DogSab.Platform.Core.Abstractions.Services;

namespace DogSab.Platform.Core.Impl.DependencyInjection;

/// <summary>
/// Extension helpers for <see cref="IServiceContainer"/> used to bridge into
/// APIs that only have the runtime <see cref="Type"/> available, not a generic parameter
/// (e.g. <see cref="IServiceProvider"/> adapters).
/// </summary>
internal static class ServiceContainerExtensions
{
    /// <summary>
    /// Attempts to resolve a service by its runtime type without throwing if it is not registered.
    /// </summary>
    /// <param name="container">The container to resolve from.</param>
    /// <param name="serviceType">The service interface type to resolve.</param>
    /// <param name="instance">The resolved instance, or <c>null</c> if not registered.</param>
    /// <returns><c>true</c> if the service was found; otherwise <c>false</c>.</returns>
    public static bool TryGetServiceUntyped(this IServiceContainer container, Type serviceType, out object? instance)
    {
        try
        {
            instance = container.GetService(serviceType);
            return true;
        }
        catch (ServiceResolutionException)
        {
            instance = null;
            return false;
        }
    }
}