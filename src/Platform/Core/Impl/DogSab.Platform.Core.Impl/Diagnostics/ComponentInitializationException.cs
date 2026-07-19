namespace DogSab.Platform.Core.Impl.Diagnostics;

/// <summary>
/// Thrown when a component's <c>InitComponent()</c> (or, for project components,
/// <c>ProjectOpened()</c>) throws during platform-driven initialization.
/// Wraps the original exception with the failing component's type for diagnostics.
/// </summary>
public sealed class ComponentInitializationException : Exception
{
    /// <summary>The component interface type whose initialization failed.</summary>
    public Type ComponentType { get; }

    /// <summary>
    /// Creates a new exception describing a component initialization failure.
    /// </summary>
    /// <param name="componentType">The component interface type whose initialization failed.</param>
    /// <param name="inner">The exception thrown by the component's init hook.</param>
    public ComponentInitializationException(Type componentType, Exception inner)
        : base($"Component '{componentType.FullName}' failed to initialize: {inner.Message}", inner)
    {
        ComponentType = componentType;
    }
}