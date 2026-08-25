using DogSab.Platform.Psi.Abstractions.Tree;

namespace DogSab.Platform.Editor.Highlighting;

/// <summary>
/// Computes syntax highlighting spans for a file, given its PSI tree.
/// Implemented per language and registered against
/// <see cref="Events.HighlightingExtensionPoints.SYNTAX_HIGHLIGHTER"/>.
/// Takes the already-parsed <see cref="IPsiFile"/> rather than raw text or
/// tokens directly, so highlighting can be semantically aware (e.g.
/// distinguishing a type name from a regular identifier by walking the
/// tree) rather than being purely lexical — a purely lexical highlighter
/// can still be implemented trivially against this same contract by simply
/// walking the file's tokens without using the tree structure at all.
/// </summary>
public interface ISyntaxHighlighter
{
    /// <summary>
    /// Computes the highlight spans for a file.
    /// </summary>
    /// <param name="psiFile">
    /// The file's parsed PSI tree.
    /// </param>
    /// <returns>
    /// The highlight spans found, covering whichever portions of the text
    /// this highlighter assigns a category to — spans need not cover the
    /// entire file, and uncovered ranges are simply left unstyled.
    /// </returns>
    IEnumerable<HighlightSpan> ComputeHighlighting(IPsiFile psiFile);
}