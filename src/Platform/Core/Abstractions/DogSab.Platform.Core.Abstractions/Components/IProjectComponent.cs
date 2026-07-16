namespace DogSab.Platform.Core.Abstractions.Components;

public interface IProjectComponent : IComponent
{
    void InitComponent();
    void DisposeComponent();
    
    void ProjectOpened();
    void ProjectClosed();
}