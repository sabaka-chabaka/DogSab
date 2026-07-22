using System.Collections.Concurrent;
using DogSab.Platform.Core.Abstractions.Components;
using DogSab.Platform.Core.Abstractions.Exceptions;

namespace DogSab.Platform.Core.Impl.Components;

/// <summary>
/// Manages components whose lifecycle spans a single open project.
/// A new instance of this manager is created for every opened project and
/// discarded when that project is closed, along with all its components.
/// </summary>
public class ProjectComponentManager : IComponentManager
{
    /// <summary>Registered project-component descriptors keyed by their interface type.</summary>
    private readonly ConcurrentDictionary<Type, ComponentDescriptor> _descriptors = new();

    /// <summary>Live project-component instances keyed by their interface type.</summary>
    private readonly ConcurrentDictionary<Type, IProjectComponent> _instances = new();

    /// <summary>Resolves the correct initialization order for interdependent components.</summary>
    private readonly ComponentDependencyResolver _dependencyResolver;

    /// <summary>
    /// Creates a new project-scoped component manager.
    /// </summary>
    /// <param name="dependencyResolver">Resolver used to order component initialization by dependency.</param>
    public ProjectComponentManager(ComponentDependencyResolver dependencyResolver)
    {
        _dependencyResolver = dependencyResolver;
    }

    /// <summary>
    /// Registers a project-component implementation for the given interface.
    /// </summary>
    /// <typeparam name="TInterface">The component interface exposed to consumers.</typeparam>
    /// <typeparam name="TImplementation">The concrete implementation type.</typeparam>
    public void RegisterComponent<TInterface, TImplementation>()
        where TInterface : class, IComponent
        where TImplementation : class, TInterface
    {
        if (!typeof(IProjectComponent).IsAssignableFrom(typeof(TImplementation)))
        {
            throw new ArgumentException(
                $"'{typeof(TImplementation).FullName}' must implement {nameof(IProjectComponent)} " +
                $"to be registered with {nameof(ProjectComponentManager)}.");
        }

        _descriptors[typeof(TInterface)] = new ComponentDescriptor(typeof(TInterface), typeof(TImplementation));
    }

    /// <summary>
    /// Resolves a project-component instance, creating and initializing it on first access.
    /// </summary>
    /// <typeparam name="T">The component interface to resolve.</typeparam>
    /// <returns>The singleton instance implementing <typeparamref name="T"/> for this project.</returns>
    public T GetComponent<T>() where T : class, IComponent
    {
        return (T)GetComponent(typeof(T));
    }

    /// <summary>
    /// Attempts to resolve a project-component instance without throwing if it is missing.
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
    /// Resolves a project-component instance by its runtime type.
    /// </summary>
    /// <param name="componentType">The component interface type to resolve.</param>
    /// <returns>The singleton instance implementing <paramref name="componentType"/> for this project.</returns>
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
    /// Returns the current lifecycle state of a registered project component.
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
    /// Calls <see cref="IProjectComponent.ProjectOpened"/> on every currently initialized
    /// component. Invoked once by the platform after all project components have been created.
    /// </summary>
    public void NotifyProjectOpened()
    {
        foreach (var instance in _instances.Values)
        {
            instance.ProjectOpened();
        }
    }

    /// <summary>
    /// Calls <see cref="IProjectComponent.ProjectClosed"/> and then
    /// <see cref="IProjectComponent.DisposeComponent"/> on every initialized component,
    /// in reverse initialization order. Invoked once by the platform when the project closes.
    /// </summary>
    public virtual void DisposeAll()
    {
        foreach (var instance in _instances.Values)
        {
            instance.ProjectClosed();
        }

        foreach (var instance in _instances.Values)
        {
            instance.DisposeComponent();
        }

        _instances.Clear();
    }

    /// <summary>
    /// Creates an instance of the component described by <paramref name="descriptor"/>,
    /// resolving its dependencies in the correct order and calling <c>InitComponent</c>.
    /// Does not call <c>ProjectOpened</c> — that happens later, once for all components,
    /// via <see cref="NotifyProjectOpened"/>.
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

            var instance = (IProjectComponent)Activator.CreateInstance(current.ImplementationType)!;
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