namespace DogSab.Platform.Core.Abstractions.Lifecycle;

public interface IDisposableRegistry
{
    void Register(IDisposable parent, IDisposable child);
    void Unregister(IDisposable child);
    bool IsDisposed(IDisposable target);
}