namespace DogSab.Platform.Core.Abstractions.Components;

/// <summary>
/// Declares that a component implementation must be initialized only after
/// the specified dependency component interfaces have already been initialized.
/// Applied to the implementation class, not the interface.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class ComponentDependsOnAttribute : Attribute
{
    /// <summary>The component interface type that must be initialized first.</summary>
    public Type DependencyInterfaceType { get; }

    /// <summary>
    /// Declares a dependency on another component.
    /// </summary>
    /// <param name="dependencyInterfaceType">The component interface type that must be initialized first.</param>
    public ComponentDependsOnAttribute(Type dependencyInterfaceType)
    {
        DependencyInterfaceType = dependencyInterfaceType;
    }
}