using System.Collections.Concurrent;
using DogSab.Platform.Core.Abstractions.Components;
using DogSab.Platform.Core.Abstractions.Exceptions;

namespace DogSab.Platform.Core.Impl.Components;

/// <summary>
/// Manages components whose lifecycle spans the entire application process.
/// A single instance of this manager exists for the lifetime of the process;
/// components registered here are created once at startup and disposed on shutdown.
/// </summary>
public class ApplicationComponentManager : IComponentManager
{
    /// <summary>Registered application-component descriptors keyed by their interface type.</summary>
    private readonly ConcurrentDictionary<Type, ComponentDescriptor> _descriptors = new();

    /// <summary>Live application-component instances keyed by their interface type.</summary>
    private readonly ConcurrentDictionary<Type, IApplicationComponent> _instances = new();

    /// <summary>Resolves the correct initialization order for interdependent components.</summary>
    private readonly ComponentDependencyResolver _dependencyResolver;

    /// <summary>
    /// Creates a new application-scoped component manager.
    /// </summary>
    /// <param name="dependencyResolver">Resolver used to order component initialization by dependency.</param>
    public ApplicationComponentManager(ComponentDependencyResolver dependencyResolver)
    {
        _dependencyResolver = dependencyResolver;
    }

    /// <summary>
    /// Registers an application-component implementation for the given interface.
    /// </summary>
    /// <typeparam name="TInterface">The component interface exposed to consumers.</typeparam>
    /// <typeparam name="TImplementation">The concrete implementation type.</typeparam>
    public void RegisterComponent<TInterface, TImplementation>()
        where TInterface : class, IComponent
        where TImplementation : class, TInterface
    {
        if (!typeof(IApplicationComponent).IsAssignableFrom(typeof(TImplementation)))
        {
            throw new ArgumentException(
                $"'{typeof(TImplementation).FullName}' must implement {nameof(IApplicationComponent)} " +
                $"to be registered with {nameof(ApplicationComponentManager)}.");
        }

        _descriptors[typeof(TInterface)] = new ComponentDescriptor(typeof(TInterface), typeof(TImplementation));
    }

    /// <summary>
    /// Resolves an application-component instance, creating and initializing it on first access.
    /// </summary>
    /// <typeparam name="T">The component interface to resolve.</typeparam>
    /// <returns>The singleton instance implementing <typeparamref name="T"/>.</returns>
    public T GetComponent<T>() where T : class, IComponent
    {
        return (T)GetComponent(typeof(T));
    }

    /// <summary>
    /// Attempts to resolve an application-component instance without throwing if it is missing.
    /// </summary>
    /// <typeparam name="T">The component interface to resolve.</typeparam>
    /// <param name="component">The resolved instance, or <c>null</c> if not registered.</param>
    /// <returns><c>true</c> if the component was found; otherwise <c>false</c>.</returns>
    public bool TryGetComponent<T>(out T? component) where T : class, IComponent
    {
        if (!_descriptors.ContainsKey(typeof(T)))
        {
            component = null;
            return false;
        }

        component = (T)GetComponent(typeof(T));
        return true;
    }

    /// <summary>
    /// Resolves an application-component instance by its runtime type.
    /// </summary>
    /// <param name="componentType">The component interface type to resolve.</param>
    /// <returns>The singleton instance implementing <paramref name="componentType"/>.</returns>
    public object GetComponent(Type componentType)
    {
        if (_instances.TryGetValue(componentType, out var existing))
        {
            return existing;
        }

        if (!_descriptors.TryGetValue(componentType, out var descriptor))
        {
            throw new ComponentNotFoundException(componentType);
        }

        return CreateAndInitialize(descriptor);
    }

    /// <summary>
    /// Returns the current lifecycle state of a registered application component.
    /// </summary>
    /// <param name="componentType">The component interface type to query.</param>
    /// <returns>The current <see cref="ComponentLifecycleState"/> of the component.</returns>
    public ComponentLifecycleState GetState(Type componentType)
    {
        if (!_descriptors.TryGetValue(componentType, out var descriptor))
        {
            throw new ComponentNotFoundException(componentType);
        }

        return descriptor.State;
    }

    /// <summary>
    /// Disposes all currently initialized application components, in reverse
    /// initialization order, calling <see cref="IApplicationComponent.DisposeComponent"/> on each.
    /// Invoked once by the platform during process shutdown.
    /// </summary>
    public virtual void DisposeAll()
    {
        foreach (var instance in _instances.Values)
        {
            instance.DisposeComponent();
        }

        _instances.Clear();
    }

    /// <summary>
    /// Creates an instance of the component described by <paramref name="descriptor"/>,
    /// resolving its dependencies in the correct order and calling <c>InitComponent</c>.
    /// </summary>
    /// <param name="descriptor">The descriptor of the component to instantiate.</param>
    /// <returns>The newly created and initialized component instance.</returns>
    private object CreateAndInitialize(ComponentDescriptor descriptor)
    {
        descriptor.State = ComponentLifecycleState.Initializing;

        var orderedDescriptors = _dependencyResolver.ResolveOrder(descriptor, _descriptors);

        object? result = null;

        foreach (var current in orderedDescriptors)
        {
            if (_instances.ContainsKey(current.InterfaceType))
            {
                continue;
            }

            var instance = (IApplicationComponent)Activator.CreateInstance(current.ImplementationType)!;
            instance.InitComponent();

            current.State = ComponentLifecycleState.Initialized;
            _instances[current.InterfaceType] = instance;

            if (current.InterfaceType == descriptor.InterfaceType)
            {
                result = instance;
            }
        }

        return result!;
    }
}