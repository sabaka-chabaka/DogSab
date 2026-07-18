namespace DogSab.Platform.Core.Impl.Disposables;

/// <summary>
/// A node in the ownership tree maintained by <see cref="DisposableRegistryImpl"/>.
/// Wraps a single registered <see cref="IDisposable"/> together with the set of
/// child nodes registered under it, so the tree can be walked and torn down
/// without requiring the disposable itself to track its own children.
/// </summary>
internal sealed class DisposableNode
{
    /// <summary>The disposable object this node represents.</summary>
    public IDisposable Target { get; }

    /// <summary>The parent node, or <c>null</c> if this is a root (no registered parent).</summary>
    public DisposableNode? Parent { get; set; }

    /// <summary>Child nodes registered under this one, in registration order.</summary>
    public List<DisposableNode> Children { get; } = new();

    /// <summary>Whether <see cref="Target"/> has already been disposed by the registry.</summary>
    public bool IsDisposed { get; set; }

    /// <summary>
    /// Creates a new tree node wrapping a disposable target.
    /// </summary>
    /// <param name="target">The disposable object this node represents.</param>
    public DisposableNode(IDisposable target)
    {
        Target = target;
    }
}