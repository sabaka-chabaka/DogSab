namespace DogSab.Platform.Core.Abstractions.Components;

public interface IComponentManager
{
    T GetComponent<T>() where T : class, IComponent;
    bool TryGetComponent<T>(out T? component) where T : class, IComponent;
    object GetComponent(Type componentType);

    ComponentLifecycleState GetState(Type componentType);

    void RegisterComponent<TInterface, TImplementation>()
        where TInterface : class, IComponent
        where TImplementation : class, TInterface;
}