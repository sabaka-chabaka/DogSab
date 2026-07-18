namespace DogSab.Platform.Core.Abstractions.Services;

/// <summary>Contract for the platform's service locator / DI container.</summary>
public interface IServiceContainer
{
    /// <summary>
    /// The parent container this container delegates to when a requested service
    /// is not registered in its own scope, or <c>null</c> if this is the root
    /// (application-level) container.
    /// </summary>
    IServiceContainer? Parent { get; }
    
    /// <summary>
    /// Resolves a service instance by its interface type, creating it lazily if needed.
    /// </summary>
    /// <typeparam name="T">The service interface to resolve.</typeparam>
    /// <returns>The resolved service instance.</returns>
    T GetService<T>() where T : class, IService;

    /// <summary>
    /// Attempts to resolve a service instance without throwing if it is not registered.
    /// </summary>
    /// <typeparam name="T">The service interface to resolve.</typeparam>
    /// <param name="service">The resolved instance, or <c>null</c> if not registered.</param>
    /// <returns><c>true</c> if the service was found; otherwise <c>false</c>.</returns>
    bool TryGetService<T>(out T? service) where T : class, IService;

    /// <summary>
    /// Resolves a service instance by its runtime type.
    /// </summary>
    /// <param name="serviceType">The service interface type to resolve.</param>
    /// <returns>The resolved service instance.</returns>
    object GetService(Type serviceType);

    /// <summary>
    /// Registers a service implementation with the container.
    /// </summary>
    /// <param name="registration">Description of the service, its implementation, scope and lifetime.</param>
    void RegisterService(ServiceRegistration registration);

    /// <summary>
    /// Checks whether a service interface has a registered implementation.
    /// </summary>
    /// <typeparam name="T">The service interface to check.</typeparam>
    /// <returns><c>true</c> if a registration exists; otherwise <c>false</c>.</returns>
    bool IsRegistered<T>() where T : class, IService;
}