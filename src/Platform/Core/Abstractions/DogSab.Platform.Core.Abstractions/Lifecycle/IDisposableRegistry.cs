namespace DogSab.Platform.Core.Abstractions.Lifecycle;

/// <summary>
/// A registry of disposable objects with an explicit ownership hierarchy.
/// When a parent is disposed, all its registered children are automatically disposed
/// (analogous to the Disposer mechanism in IntelliJ Platform).
/// </summary>
public interface IDisposableRegistry
{
    /// <summary>
    /// Registers <paramref name="child"/> as owned by <paramref name="parent"/>,
    /// so it is disposed automatically when the parent is disposed.
    /// </summary>
    /// <param name="parent">The owning disposable.</param>
    /// <param name="child">The dependent disposable to register.</param>
    void Register(IDisposable parent, IDisposable child);

    /// <summary>
    /// Removes a previously registered child from the ownership tree without disposing it.
    /// </summary>
    /// <param name="child">The disposable to unregister.</param>
    void Unregister(IDisposable child);

    /// <summary>
    /// Checks whether the given object has already been disposed by the registry.
    /// </summary>
    /// <param name="target">The disposable to check.</param>
    /// <returns><c>true</c> if it has already been disposed; otherwise <c>false</c>.</returns>
    bool IsDisposed(IDisposable target);
}