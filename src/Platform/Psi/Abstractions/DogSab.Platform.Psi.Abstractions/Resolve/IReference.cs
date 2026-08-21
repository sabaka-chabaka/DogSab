using DogSab.Platform.Psi.Abstractions.Tree;

namespace DogSab.Platform.Psi.Abstractions.Resolve;

/// <summary>
/// A reference from one point in the source text (typically a single
/// <see cref="IPsiElement"/>, like an identifier) to the element it refers
/// to elsewhere — e.g. a usage of a class name referring back to that
/// class's declaration. The mechanism behind "Go to Declaration" and "Find
/// Usages": a reference knows how to resolve itself, and a reverse index
/// (built by scanning all references, likely via an
/// <c>Indexing.Abstractions.Index.IIndexExtension</c> in a future language
/// plugin) lets the platform find every reference pointing at a given
/// declaration. Implemented per language, since how a reference is resolved
/// — by scope rules, imports, overload resolution — is entirely
/// language-specific and unknown to the platform itself.
/// </summary>
public interface IReference
{
    /// <summary>The element this reference originates from (e.g. the identifier token being referenced).</summary>
    IPsiElement Source { get; }

    /// <summary>
    /// Attempts to resolve this reference to the element it points to.
    /// </summary>
    /// <returns>The resolve outcome — either the target element, or a failure.</returns>
    ResolveResult Resolve();
}