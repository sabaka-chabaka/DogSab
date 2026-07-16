namespace DogSab.Platform.Core.Abstractions.Components;

/// <summary>
/// A component that lives for the lifetime of a single open project.
/// Created when the project is opened, disposed when it is closed.
/// </summary>
public interface IProjectComponent : IComponent
{
    /// <summary>Called once by the platform right after the component instance is created for a project.</summary>
    void InitComponent();

    /// <summary>Called once by the platform when the owning project is being closed, to release resources.</summary>
    void DisposeComponent();

    /// <summary>Called after all components of the project have been initialized.</summary>
    void ProjectOpened();

    /// <summary>Called before the project starts closing, while all components are still valid.</summary>
    void ProjectClosed();
}