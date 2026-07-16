namespace DogSab.Platform.Core.Abstractions.Services;

public interface IServiceContainer
{
    T GetService<T>() where T : class, IService;
    bool TryGetService<T>(out T? service) where T : class, IService;
    object GetService(Type serviceType);

    void RegisterService(ServiceRegistration registration);
    bool IsRegistered<T>() where T : class, IService;
}