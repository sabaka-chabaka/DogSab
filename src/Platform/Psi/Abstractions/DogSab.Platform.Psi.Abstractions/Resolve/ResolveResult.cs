using DogSab.Platform.Psi.Abstractions.Tree;

namespace DogSab.Platform.Psi.Abstractions.Resolve;

/// <summary>
/// The outcome of resolving an <see cref="IReference"/>: either the
/// <see cref="IPsiElement"/> the reference points to, or nothing if
/// resolution failed (e.g. the referenced symbol doesn't exist, or the
/// project is still in dumb mode and the relevant index isn't ready yet).
/// A struct rather than a plain nullable <see cref="IPsiElement"/>, so a
/// future version can add diagnostic detail (e.g. why resolution failed)
/// without changing every call site's signature.
/// </summary>
public readonly struct ResolveResult
{
    /// <summary>The resolved element, or <c>null</c> if resolution failed.</summary>
    public IPsiElement? Element { get; }

    /// <summary>Whether resolution succeeded — equivalent to <c><see cref="Element"/> is not null</c>.</summary>
    public bool IsResolved => Element is not null;

    private ResolveResult(IPsiElement? element)
    {
        Element = element;
    }

    /// <summary>Creates a successful result pointing to the given element.</summary>
    /// <param name="element">The resolved element.</param>
    /// <returns>A successful resolve result.</returns>
    public static ResolveResult Resolved(IPsiElement element) => new(element);

    /// <summary>Creates a failed result, indicating the reference could not be resolved.</summary>
    /// <returns>A failed resolve result.</returns>
    public static ResolveResult Unresolved() => new(null);
}