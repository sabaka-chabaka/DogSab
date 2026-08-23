namespace DogSab.Platform.Editor.Abstractions.Caret;

/// <summary>
/// Tracks one or more caret (cursor) positions within a document, supporting
/// multi-cursor editing from the start rather than retrofitting it later.
/// <see cref="PrimaryCaret"/> is always the first entry in
/// <see cref="AllCarets"/> — the one driving single-cursor-oriented features
/// (e.g. "show completion at the caret") when multi-cursor isn't relevant to
/// that particular feature.
/// </summary>
public interface ICaretModel
{
    /// <summary>The primary caret's current position.</summary>
    TextPosition PrimaryCaret { get; }

    /// <summary>Every active caret position, in document order, with <see cref="PrimaryCaret"/> always first.</summary>
    IReadOnlyList<TextPosition> AllCarets { get; }

    /// <summary>
    /// Moves the primary caret to a new position, clearing any additional
    /// carets (collapsing back to single-cursor mode).
    /// </summary>
    /// <param name="position">The position to move the primary caret to.</param>
    void MoveTo(TextPosition position);

    /// <summary>
    /// Adds an additional caret at the given position, entering or extending
    /// multi-cursor mode. If a caret already exists at this position, this is a no-op.
    /// </summary>
    /// <param name="position">The position to add a caret at.</param>
    void AddCaret(TextPosition position);

    /// <summary>
    /// Removes all carets except the primary one, returning to single-cursor mode.
    /// </summary>
    void CollapseToPrimary();
}