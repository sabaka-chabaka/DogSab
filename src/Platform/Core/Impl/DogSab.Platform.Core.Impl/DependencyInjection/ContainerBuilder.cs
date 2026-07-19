using DogSab.Platform.Core.Abstractions.Services;
using DogSab.Platform.Core.Impl.Services;

namespace DogSab.Platform.Core.Impl.DependencyInjection;

/// <summary>
/// Fluent builder that assembles the platform's application-level (root)
/// <see cref="System.ComponentModel.Design.IServiceContainer"/> at startup, wiring together the registry,
/// circular-dependency detector, and activator it depends on internally.
/// </summary>
public sealed class ContainerBuilder
{
    /// <summary>Registrations accumulated so far, applied to the container once built.</summary>
    private readonly ServiceRegistry _registry = new();

    /// <summary>
    /// Registers a service implementation to ber added once <see cref="Build"/> is called."/>
    /// </summary>
    /// <param name="lifetime">How instances are created and reused.</param>
    /// <typeparam name="TInterface">The public interface consumers will request.</typeparam>
    /// <typeparam name="TImplementation">The concrete implementation type.</typeparam>
    /// <returns>This builder, for chaining.</returns>
    public ContainerBuilder AddService<TInterface, TImplementation>(
        ServiceLifetime lifetime = ServiceLifetime.Singleton)
        where TInterface : class, IService
        where TImplementation : class, TInterface
    {
        _registry.Add(new ServiceRegistration(typeof(TInterface), typeof(TImplementation), ServiceScope.Application, lifetime));
        
        return this;
    }
    
    /// <summary>
    /// Builds the root application-level service container with all registrations
    /// accumulated so far.
    /// </summary>
    /// <returns>A new root <see cref="ServiceContainerImpl"/> with no parent.</returns>
    public ServiceContainerImpl Build()
    {
        var activator = new ServiceActivator(new ServiceCircularDependencyDetector());
        return new ServiceContainerImpl(_registry, activator);
    }
}