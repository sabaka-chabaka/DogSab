using DogSab.Platform.Core.Abstractions.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DogSab.Platform.Core.Impl.DependencyInjection;

/// <summary>
/// Extension methods for registering the platform's own service registrations
/// into a standard <see cref="IServiceCollection"/>, for the (less common)
/// direction of integration: a third-party component built on top of
/// Microsoft.Extensions.DependencyInjection that needs access to platform services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers a bridge into the given <see cref="IServiceCollection"/> that resolves
    /// any requested type through the platform's <see cref="IServiceContainer"/> via
    /// <see cref="MicrosoftDiAdapter"/>, as a fallback when a type is not otherwise
    /// registered directly with the collection.
    /// </summary>
    /// <param name="services">The service collection to extend.</param>
    /// <param name="platformContainer">The platform container to bridge into.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance, for chaining.</returns>
    public static IServiceCollection AddDogSabPlatformBridge(
        this IServiceCollection services,
        IServiceContainer platformContainer)
    {
        services.AddSingleton<IServiceProvider>(new MicrosoftDiAdapter(platformContainer));
        return services;
    }
}