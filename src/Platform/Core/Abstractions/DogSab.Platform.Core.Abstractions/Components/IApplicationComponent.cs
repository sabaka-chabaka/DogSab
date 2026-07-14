namespace DogSab.Platform.Core.Abstractions.Components;

public interface IApplicationComponent : IComponent
{
    void InitComponent();
    void DisposeComponent();
}