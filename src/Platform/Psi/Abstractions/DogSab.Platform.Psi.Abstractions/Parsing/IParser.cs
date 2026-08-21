using DogSab.Platform.Psi.Abstractions.Lexing;
using DogSab.Platform.Psi.Abstractions.Tree;

namespace DogSab.Platform.Psi.Abstractions.Parsing;

/// <summary>
/// Assembles a flat token sequence (produced by an <see cref="ILexer"/>)
/// into a structured PSI tree. Implemented per language by a language
/// plugin; the platform itself has no built-in parser and does not interpret
/// the resulting tree's structure — it only knows how to walk the generic
/// <see cref="IPsiElement"/> shape.
/// </summary>
public interface IParser
{
    /// <summary>
    /// Parses a token sequence into a PSI tree, returning its root node
    /// (before it is wrapped as a full <see cref="IPsiFile"/> by the platform's
    /// PSI building infrastructure).
    /// </summary>
    /// <param name="tokens">The tokens to parse, as produced by the matching <see cref="ILexer"/> for the same source text.</param>
    /// <param name="sourceText">The original source text, needed to compute each produced node's <see cref="IPsiElement.Text"/>.</param>
    /// <returns>The root <see cref="IPsiElement"/> of the resulting tree.</returns>
    IPsiElement Parse(IEnumerable<IToken> tokens, string sourceText);
}