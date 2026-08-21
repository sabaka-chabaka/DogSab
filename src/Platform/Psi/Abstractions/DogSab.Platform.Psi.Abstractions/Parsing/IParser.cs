using DogSab.Platform.Psi.Abstractions.Lexing;
using DogSab.Platform.Psi.Abstractions.Tree;

namespace DogSab.Platform.Psi.Abstractions.Parsing;

/// <summary>
/// Assembles a flat token sequence into a structured PSI tree, directly under
/// a platform-provided file root — rather than returning a separate,
/// language-owned root that the platform would then need to graft onto its
/// own tree (which previously risked silently losing nodes if a language
/// plugin returned an <see cref="IPsiElement"/> implementation the platform
/// didn't recognize). Implemented per language; the platform supplies the
/// root and the language populates it.
/// </summary>
public interface IParser
{
    /// <summary>
    /// Parses a token sequence, attaching the resulting tree structure
    /// directly under <paramref name="fileRoot"/> via <see cref="IPsiTreeWriter"/>.
    /// </summary>
    /// <param name="tokens">The tokens to parse, as produced by the matching <see cref="ILexer"/> for the same source text.</param>
    /// <param name="sourceText">The original source text.</param>
    /// <param name="fileRoot">
    /// A writer bound to the platform's file root, used to append child
    /// nodes without the language needing to construct or know about the
    /// platform's concrete tree node type.
    /// </param>
    void Parse(IEnumerable<IToken> tokens, string sourceText, IPsiTreeWriter fileRoot);
}

/// <summary>
/// A narrow, write-only interface for appending nodes to a PSI tree during
/// parsing, without exposing the platform's concrete node implementation to
/// language plugins. This is the boundary that keeps <c>Psi.Abstractions</c>
/// free of any dependency on the tree node implementation in the Impl assembly.
/// </summary>
public interface IPsiTreeWriter
{
    /// <summary>
    /// Appends a new child node under the current writer's node and returns
    /// a writer scoped to that new child, for attaching further descendants.
    /// </summary>
    /// <param name="type">The kind of node to create.</param>
    /// <param name="startOffset">The zero-based character offset where the node starts.</param>
    /// <param name="length">The length, in characters, of the node's span.</param>
    /// <returns>A writer for the newly created child node.</returns>
    IPsiTreeWriter AppendChild(Tree.PsiElementType type, int startOffset, int length);
}