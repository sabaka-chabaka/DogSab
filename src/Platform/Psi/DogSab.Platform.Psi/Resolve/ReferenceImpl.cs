using DogSab.Platform.Psi.Abstractions.Resolve;
using DogSab.Platform.Psi.Abstractions.Tree;

namespace DogSab.Platform.Psi.Resolve;

/// <summary>
/// Default implementation of <see cref="IReference"/>, delegating actual
/// resolution to a supplied delegate. Language plugins typically construct
/// one of these per reference-bearing PSI element, supplying language-specific
/// resolution logic (scope lookup, import resolution, etc.) as the delegate,
/// rather than implementing <see cref="IReference"/> themselves from scratch.
/// </summary>
public sealed class ReferenceImpl : IReference
{
    private readonly Func<ResolveResult> _resolveFunc;

    /// <inheritdoc />
    public IPsiElement Source { get; }

    /// <summary>
    /// Creates a new reference.
    /// </summary>
    /// <param name="source">The element this reference originates from.</param>
    /// <param name="resolveFunc">The language-specific resolution logic.</param>
    public ReferenceImpl(IPsiElement source, Func<ResolveResult> resolveFunc)
    {
        Source = source;
        _resolveFunc = resolveFunc;
    }

    /// <inheritdoc />
    public ResolveResult Resolve() => _resolveFunc();
}