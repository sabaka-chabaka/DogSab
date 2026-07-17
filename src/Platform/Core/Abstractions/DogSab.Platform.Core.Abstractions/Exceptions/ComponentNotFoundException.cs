namespace DogSab.Platform.Core.Abstractions.Exceptions;

/// <summary>Thrown when a requested component has not been registered with the component manager.</summary>
public sealed class ComponentNotFoundException : Exception
{
    /// <summary>The component type that could not be resolved.</summary>
    public Type ComponentType { get; }

    /// <summary>
    /// Creates a new exception for a missing component.
    /// </summary>
    /// <param name="componentType">The component type that could not be resolved.</param>
    public ComponentNotFoundException(Type componentType)
        : base($"Component of type '{componentType.FullName}' is not registered.")
    {
        ComponentType = componentType;
    }
}