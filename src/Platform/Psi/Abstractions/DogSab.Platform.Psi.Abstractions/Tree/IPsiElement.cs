namespace DogSab.Platform.Psi.Abstractions.Tree;

/// <summary>
/// A single node in a PSI tree: a classified span of source text, with
/// parent/child relationships to neighboring nodes, forming a structured
/// representation of a file's syntax. Built by a language's
/// <see cref="Parsing.IParser"/> from the flat token sequence produced by
/// its <see cref="Lexing.ILexer"/>. Platform features (folding, structure
/// view, refactoring) operate against this tree generically, regardless of
/// which language produced it, since every language's tree is made of the
/// same <see cref="IPsiElement"/> shape — only the specific
/// <see cref="Type"/> values differ per language.
/// </summary>
public interface IPsiElement
{
    /// <summary>The kind of node this is.</summary>
    PsiElementType Type { get; }

    /// <summary>The zero-based character offset into the file's text where this node starts.</summary>
    int StartOffset { get; }

    /// <summary>The length, in characters, of this node's span in the file's text — covering itself and all descendants.</summary>
    int Length { get; }

    /// <summary>This node's exact source text.</summary>
    string Text { get; }

    /// <summary>The parent node, or <c>null</c> if this is the tree's root (the <see cref="IPsiFile"/> itself).</summary>
    IPsiElement? Parent { get; }

    /// <summary>This node's immediate child nodes, in source order.</summary>
    IReadOnlyList<IPsiElement> Children { get; }

    /// <summary>The file this node belongs to, walking up to the root if necessary.</summary>
    IPsiFile ContainingFile { get; }
}