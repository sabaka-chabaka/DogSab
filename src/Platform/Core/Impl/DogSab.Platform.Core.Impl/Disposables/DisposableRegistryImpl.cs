using System.Runtime.CompilerServices;
using DogSab.Platform.Core.Abstractions.Lifecycle;
using DogSab.Platform.Core.Abstractions.Logging;

namespace DogSab.Platform.Core.Impl.Disposables;

/// <summary>
/// Default implementation of <see cref="IDisposableRegistry"/>.
/// Maintains a tree of registered disposables keyed by object identity
/// (via <see cref="ConditionalWeakTable{TKey,TValue}"/> semantics, but implemented
/// with an explicit dictionary here for deterministic enumeration order).
/// When a parent is disposed through this registry, all its registered descendants
/// are disposed automatically, deepest first.
/// </summary>
public class DisposableRegistryImpl : IDisposableRegistry
{
    /// <summary>Logger used to report registration errors and disposal failures.</summary>
    private readonly ILogger _logger;

    /// <summary>All known nodes, keyed by the disposable instance they wrap.</summary>
    private readonly Dictionary<IDisposable, DisposableNode> _nodes = new(ReferenceEqualityComparer.Instance);

    /// <summary>Synchronizes all mutations of the tree, since registration can happen from any thread.</summary>
    private readonly object _lock = new();

    /// <summary>
    /// Creates a new disposable registry.
    /// </summary>
    /// <param name="loggerFactory">Factory used to obtain a logger scoped to this registry.</param>
    public DisposableRegistryImpl(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.GetLogger(typeof(DisposableRegistryImpl));
    }

    /// <summary>
    /// Registers <paramref name="child"/> as owned by <paramref name="parent"/>,
    /// so it is disposed automatically when the parent is disposed via this registry.
    /// If <paramref name="parent"/> is not yet known to the registry, it is added
    /// as a new root node.
    /// </summary>
    /// <param name="parent">The owning disposable.</param>
    /// <param name="child">The dependent disposable to register.</param>
    public void Register(IDisposable parent, IDisposable child)
    {
        lock (_lock)
        {
            var parentNode = GetOrCreateNode(parent);
            var childNode = GetOrCreateNode(child);

            if (childNode.Parent is not null)
            {
                childNode.Parent.Children.Remove(childNode);
            }

            childNode.Parent = parentNode;
            parentNode.Children.Add(childNode);
        }
    }

    /// <summary>
    /// Removes a previously registered child from the ownership tree without disposing it.
    /// If the child is unknown to the registry, this is a no-op.
    /// </summary>
    /// <param name="child">The disposable to unregister.</param>
    public void Unregister(IDisposable child)
    {
        lock (_lock)
        {
            if (!_nodes.TryGetValue(child, out var childNode))
            {
                return;
            }

            childNode.Parent?.Children.Remove(childNode);
            childNode.Parent = null;
            _nodes.Remove(child);
        }
    }

    /// <summary>
    /// Checks whether the given object has already been disposed by this registry.
    /// Returns <c>false</c> for objects the registry has never seen.
    /// </summary>
    /// <param name="target">The disposable to check.</param>
    /// <returns><c>true</c> if it has already been disposed via this registry; otherwise <c>false</c>.</returns>
    public bool IsDisposed(IDisposable target)
    {
        lock (_lock)
        {
            return _nodes.TryGetValue(target, out var node) && node.IsDisposed;
        }
    }

    /// <summary>
    /// Disposes <paramref name="target"/> and, recursively, all of its registered
    /// descendants, deepest first. Safe to call multiple times for the same target;
    /// subsequent calls are no-ops. Exceptions from individual disposals are logged
    /// and do not prevent sibling or ancestor disposal from proceeding.
    /// </summary>
    /// <param name="target">The disposable to tear down, along with its subtree.</param>
    public void DisposeTree(IDisposable target)
    {
        List<DisposableNode> subtreeDeepestFirst;

        lock (_lock)
        {
            if (!_nodes.TryGetValue(target, out var rootNode) || rootNode.IsDisposed)
            {
                return;
            }

            subtreeDeepestFirst = new List<DisposableNode>();
            CollectDeepestFirst(rootNode, subtreeDeepestFirst);
        }

        foreach (var node in subtreeDeepestFirst)
        {
            DisposeNode(node);
        }
    }

    /// <summary>
    /// Checks whether the given object is registered in the registry.
    /// Internal for testing.
    /// </summary>
    internal bool IsRegistered(IDisposable target)
    {
        lock (_lock)
        {
            return _nodes.ContainsKey(target);
        }
    }

    /// <summary>
    /// Finds or creates the tree node for a given disposable instance.
    /// Callers must hold <see cref="_lock"/>.
    /// </summary>
    /// <param name="target">The disposable to find or wrap.</param>
    /// <returns>The existing or newly created node for <paramref name="target"/>.</returns>
    private DisposableNode GetOrCreateNode(IDisposable target)
    {
        if (_nodes.TryGetValue(target, out var existing))
        {
            return existing;
        }

        var node = new DisposableNode(target);
        _nodes[target] = node;
        return node;
    }

    /// <summary>
    /// Performs a post-order traversal of the subtree rooted at <paramref name="node"/>,
    /// appending nodes to <paramref name="result"/> such that every child appears
    /// before its parent (deepest first). Callers must hold <see cref="_lock"/>.
    /// </summary>
    /// <param name="node">The subtree root to traverse.</param>
    /// <param name="result">The accumulator list receiving nodes in disposal order.</param>
    private static void CollectDeepestFirst(DisposableNode node, List<DisposableNode> result)
    {
        foreach (var child in node.Children)
        {
            if (!child.IsDisposed)
            {
                CollectDeepestFirst(child, result);
            }
        }

        result.Add(node);
    }

    /// <summary>
    /// Disposes a single node's target, marks it disposed, and removes it from the registry.
    /// Any exception thrown by the target's <c>Dispose()</c> is logged and swallowed so that
    /// the rest of the subtree still gets torn down.
    /// </summary>
    /// <param name="node">The node whose target should be disposed.</param>
    private void DisposeNode(DisposableNode node)
    {
        lock (_lock)
        {
            if (node.IsDisposed)
            {
                return;
            }

            node.IsDisposed = true;
        }

        try
        {
            node.Target.Dispose();
        }
        catch (Exception ex)
        {
            _logger.Error(
                "Exception while disposing '{0}' registered in the disposable tree",
                ex,
                node.Target.GetType().FullName ?? node.Target.GetType().Name);
        }

        lock (_lock)
        {
            node.Parent?.Children.Remove(node);
            node.Parent = null;
            _nodes.Remove(node.Target);
        }
    }
}