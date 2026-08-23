namespace DogSab.Platform.Editor.Abstractions.Folding;

/// <summary>
/// A collapsible range of text in the editor gutter (e.g. a method body, an
/// import block, a multi-line comment). Purely a data description of what
/// can be folded and how it should be summarized when collapsed — the
/// interactive fold/unfold state (which regions are currently collapsed) is
/// tracked separately by the Editor.Ui module, not here; this contract only
/// describes what regions exist for a given file's content.
/// </summary>
public interface IFoldingRegion
{
    /// <summary>The character offset where the foldable range starts.</summary>
    int StartOffset { get; }

    /// <summary>The character offset where the foldable range ends.</summary>
    int EndOffset { get; }

    /// <summary>
    /// The text shown in place of the folded region's content when
    /// collapsed (e.g. <c>"{ ... }"</c> for a method body, or the first line
    /// of a multi-line comment).
    /// </summary>
    string PlaceholderText { get; }

    /// <summary>Whether this region should start collapsed by default when a file is first opened (e.g. often true for license header comments, false for method bodies).</summary>
    bool CollapsedByDefault { get; }
}