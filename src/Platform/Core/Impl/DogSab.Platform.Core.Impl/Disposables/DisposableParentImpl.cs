namespace DogSab.Platform.Core.Impl.Disposables;

/// <summary>
/// Base implementation of <see cref="Abstractions.Disposables.IDisposableParent"/>
/// for types that prefer local ownership of their children instead of registering
/// them with the global <see cref="DisposableRegistryImpl"/>. Useful for small,
/// self-contained objects (e.g. a single tool window) that do not need to
/// participate in the platform-wide ownership tree.
/// Derive from this class and call <see cref="RegisterChild"/> for owned disposables;
/// override <see cref="DisposeCore"/> instead of <c>Dispose</c> to add custom teardown logic.
/// </summary>
public abstract class DisposableParentImpl : Abstractions.Disposables.IDisposableParent
{
    /// <summary>Children registered under this instance, in registration order.</summary>
    private readonly List<IDisposable> _children = new();

    /// <summary>Whether this instance has already been disposed.</summary>
    private bool _isDisposed;

    /// <inheritdoc />
    public void RegisterChild(IDisposable child)
    {
        if (_isDisposed)
        {
            child.Dispose();
            return;
        }

        _children.Add(child);
    }

    /// <inheritdoc />
    public void UnregisterChild(IDisposable child)
    {
        _children.Remove(child);
    }

    /// <summary>
    /// Disposes this instance's own resources, then disposes all registered children
    /// in reverse registration order (last-registered, first-disposed). Safe to call
    /// multiple times; subsequent calls are no-ops.
    /// </summary>
    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;

        DisposeCore();

        for (var i = _children.Count - 1; i >= 0; i--)
        {
            _children[i].Dispose();
        }

        _children.Clear();
    }

    /// <summary>
    /// Override to release this instance's own resources. Called before any
    /// registered children are disposed. Default implementation does nothing.
    /// </summary>
    protected virtual void DisposeCore()
    {
    }
}