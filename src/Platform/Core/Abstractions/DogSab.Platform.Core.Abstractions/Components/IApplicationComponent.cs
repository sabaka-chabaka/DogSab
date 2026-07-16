namespace DogSab.Platform.Core.Abstractions.Components;

/// <summary>
/// A component that lives for the entire lifetime of the application process.
/// Created once at startup, disposed on shutdown.
/// </summary>
public interface IApplicationComponent : IComponent
{
    /// <summary>Called once by the platform right after the component instance is created.</summary>
    void InitComponent();

    /// <summary>Called once by the platform during application shutdown to release resources.</summary>
    void DisposeComponent();
}