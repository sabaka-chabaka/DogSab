namespace DogSab.Platform.Editor.Refactoring;

/// <summary>
/// A single proposed text edit as part of a refactoring's preview, before
/// the user confirms and it is actually applied to a document.
/// Mirrors the shape of <c>Editor.Abstractions.Document.DocumentChangeEvent</c>
/// but is deliberately a distinct type — a preview edit is a proposal that
/// may never be applied, while a <c>DocumentChangeEvent</c> describes a
/// change that has already happened, and conflating the two would make it
/// easy to accidentally treat a not-yet-confirmed edit as already real.
/// </summary>
public readonly struct RefactoringPreviewEdit
{
    /// <summary>
    /// The character offset where the proposed change starts.
    /// </summary>
    public int Offset { get; }

    /// <summary>
    /// The length of the text this edit would replace.
    /// </summary>
    public int Length { get; }

    /// <summary>
    /// The text that would be inserted in place of the replaced range.
    /// </summary>
    public string NewText { get; }

    /// <summary>
    /// Creates a new proposed edit.
    /// </summary>
    /// <param name="offset">
    /// The character offset where the change starts.
    /// </param>
    /// <param name="length">
    /// The length of the text being replaced.
    /// </param>
    /// <param name="newText">
    /// The replacement text.
    /// </param>
    public RefactoringPreviewEdit(int offset, int length, string newText)
    {
        Offset = offset;
        Length = length;
        NewText = newText;
    }
}

/// <summary>
/// The full set of proposed edits a refactoring would make, shown to the
/// user for confirmation before <see cref="RefactoringCoordinator.Apply"/>
/// actually replays them against a document.
/// </summary>
public readonly struct RefactoringPreview
{
    /// <summary>
    /// A short, human-readable summary of what this refactoring would do
    /// (e.g. <c>"Rename 'foo' to 'bar' (3 usages)"</c>), shown in the
    /// confirmation UI.
    /// </summary>
    public string Summary { get; }

    /// <summary>
    /// Every edit this refactoring proposes, in no particular required
    /// order — <see cref="RefactoringCoordinator.Apply"/> is responsible
    /// for applying them in an order that keeps offsets valid (typically
    /// last-to-first in document order, so earlier edits don't shift the
    /// offsets of later ones).
    /// </summary>
    public IReadOnlyList<RefactoringPreviewEdit> Edits { get; }

    /// <summary>
    /// Creates a new refactoring preview.
    /// </summary>
    /// <param name="summary">
    /// A short, human-readable summary of the proposed change.
    /// </param>
    /// <param name="edits">
    /// Every edit this refactoring proposes.
    /// </param>
    public RefactoringPreview(string summary, IReadOnlyList<RefactoringPreviewEdit> edits)
    {
        Summary = summary;
        Edits = edits;
    }
}