namespace DogSab.Platform.Core.Abstractions.Disposables;

/// <summary>
/// A node in the ownership tree of disposable objects.
/// An entity implementing this must dispose all children registered via
/// RegisterChild when it is itself disposed.
/// </summary>
public interface IDisposableParent : IDisposable
{
    /// <summary>
    /// Registers a child disposable that must be disposed when this parent is disposed.
    /// </summary>
    /// <param name="child">The dependent disposable to register.</param>
    void RegisterChild(IDisposable child);

    /// <summary>
    /// Removes a previously registered child without disposing it.
    /// </summary>
    /// <param name="child">The disposable to unregister.</param>
    void UnregisterChild(IDisposable child);
}