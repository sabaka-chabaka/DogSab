using DogSab.Platform.Core.Abstractions.Components;

namespace DogSab.Platform.Core.Impl.Components;

/// <summary>
/// Immutable request to register a component implementation for a given interface,
/// passed into a manager before any instance is created.
/// Distinct from <see cref="ComponentDescriptor"/>, which is the mutable runtime
/// entry tracking lifecycle state once registration has been accepted.
/// </summary>
public readonly struct ComponentRegistration
{
    /// <summary>The public interface type consumers will request.</summary>
    public Type InterfaceType { get; }

    /// <summary>The concrete type that implements <see cref="InterfaceType"/>.</summary>
    public Type ImplementationType { get; }

    /// <summary>
    /// The kind of component being registered, determining which manager
    /// (application- or project-scoped) owns its lifecycle.
    /// </summary>
    public ComponentKind Kind { get; }

    /// <summary>
    /// Creates a new registration request.
    /// </summary>
    /// <param name="interfaceType">The public interface type consumers will request.</param>
    /// <param name="implementationType">The concrete type that implements <paramref name="interfaceType"/>.</param>
    /// <param name="kind">Whether this is an application- or project-scoped component.</param>
    public ComponentRegistration(Type interfaceType, Type implementationType, ComponentKind kind)
    {
        InterfaceType = interfaceType;
        ImplementationType = implementationType;
        Kind = kind;
    }
}

/// <summary>Distinguishes which lifecycle a registered component follows.</summary>
public enum ComponentKind
{
    /// <summary>Lives for the entire application process; implements <see cref="IApplicationComponent"/>.</summary>
    Application,

    /// <summary>Lives for a single open project; implements <see cref="IProjectComponent"/>.</summary>
    Project
}