namespace DogSab.Platform.Editor.Abstractions.Document;

/// <summary>
/// An immutable record of a single edit applied to an <see cref="IDocument"/>:
/// a range of the old text was replaced with new text. Every document
/// mutation — typing a character, pasting, a refactoring's programmatic
/// edit — is expressible as one of these, which is what
/// <see cref="IDocumentListener"/> subscribers observe.
/// </summary>
public readonly struct DocumentChangeEvent
{
    /// <summary>The character offset in the document where the change starts.</summary>
    public int Offset { get; }

    /// <summary>The text that was removed, replaced starting at <see cref="Offset"/>. Empty for a pure insertion.</summary>
    public string OldText { get; }

    /// <summary>The text that was inserted in place of <see cref="OldText"/>. Empty for a pure deletion.</summary>
    public string NewText { get; }

    /// <summary>The document's version number after this change was applied, for staleness checks against cached data derived from the document.</summary>
    public int NewVersion { get; }

    /// <summary>
    /// Creates a new document change event.
    /// </summary>
    /// <param name="offset">The character offset where the change starts.</param>
    /// <param name="oldText">The text that was removed.</param>
    /// <param name="newText">The text that was inserted.</param>
    /// <param name="newVersion">The document's version number after this change.</param>
    public DocumentChangeEvent(int offset, string oldText, string newText, int newVersion)
    {
        Offset = offset;
        OldText = oldText;
        NewText = newText;
        NewVersion = newVersion;
    }
}