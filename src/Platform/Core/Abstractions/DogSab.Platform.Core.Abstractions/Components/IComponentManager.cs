namespace DogSab.Platform.Core.Abstractions.Components;

/// <summary>
/// Contract for the manager responsible for the lifecycle of registered components.
/// </summary>
public interface IComponentManager
{
    /// <summary>
    /// Resolves a registered component instance by its interface type.
    /// </summary>
    /// <typeparam name="T">The component interface to resolve.</typeparam>
    /// <returns>The singleton instance implementing <typeparamref name="T"/>.</returns>
    T GetComponent<T>() where T : class, IComponent;

    /// <summary>
    /// Attempts to resolve a registered component instance without throwing if it is missing.
    /// </summary>
    /// <typeparam name="T">The component interface to resolve.</typeparam>
    /// <param name="component">The resolved instance, or <c>null</c> if not registered.</param>
    /// <returns><c>true</c> if the component was found; otherwise <c>false</c>.</returns>
    bool TryGetComponent<T>(out T? component) where T : class, IComponent;

    /// <summary>
    /// Resolves a registered component instance by its runtime type.
    /// </summary>
    /// <param name="componentType">The component interface type to resolve.</param>
    /// <returns>The singleton instance implementing <paramref name="componentType"/>.</returns>
    object GetComponent(Type componentType);

    /// <summary>
    /// Returns the current lifecycle state of a registered component.
    /// </summary>
    /// <param name="componentType">The component interface type to query.</param>
    /// <returns>The current <see cref="ComponentLifecycleState"/> of the component.</returns>
    ComponentLifecycleState GetState(Type componentType);

    /// <summary>
    /// Registers an implementation type for a component interface.
    /// </summary>
    /// <typeparam name="TInterface">The component interface exposed to consumers.</typeparam>
    /// <typeparam name="TImplementation">The concrete implementation type.</typeparam>
    void RegisterComponent<TInterface, TImplementation>()
        where TInterface : class, IComponent
        where TImplementation : class, TInterface;
}