using DogSab.Platform.Core.Abstractions.Components;

namespace DogSab.Platform.Core.Impl.Components;

/// <summary>
/// Internal registry entry descibind a registered component:
/// its public interface, its implementation type, and its current lifecycle state.
/// </summary>
public sealed class ComponentDescriptor
{
    /// <summary>The public interface type consumers request when resolving this component.</summary>
    public Type InterfaceType { get; }

    /// <summary>The concrete type that implements <see cref="InterfaceType"/>.</summary>
    public Type ImplementationType { get; }

    /// <summary>The current position of this component in its lifecycle.</summary>
    public ComponentLifecycleState State { get; set; }
    
    /// <summary>
    /// Creates a new descriptor for a registered component.
    /// </summary>
    /// <param name="interfaceType">The public interface type consumers request.</param>
    /// <param name="implementationType">The concrete type that implements <paramref name="interfaceType"/>.</param>
    public ComponentDescriptor(Type interfaceType, Type implementationType)
    {
        InterfaceType = interfaceType;
        ImplementationType = implementationType;
        State = ComponentLifecycleState.NotInitialized;
    }
}