// Selection/ISelectionModel.cs
using DogSab.Platform.Editor.Abstractions.Caret;

namespace DogSab.Platform.Editor.Abstractions.Selection;

/// <summary>
/// Tracks the currently selected text range in a document. A selection is
/// anchored at a fixed start point and extends to the caret's current
/// position — <see cref="Start"/> and <see cref="End"/> are not simply "left"
/// and "right" in document order, since a selection made by dragging right-to-left
/// has its <see cref="Start"/> after its <see cref="End"/> in the text; use
/// <see cref="NormalizedStart"/>/<see cref="NormalizedEnd"/> when document
/// order (not selection direction) is what matters, e.g. for extracting the
/// selected substring.
/// </summary>
public interface ISelectionModel
{
    /// <summary>Whether any text is currently selected (a zero-length selection counts as no selection).</summary>
    bool HasSelection { get; }

    /// <summary>The position the selection was started from — where dragging or shift-click began.</summary>
    TextPosition Start { get; }

    /// <summary>The position the selection currently extends to — typically the caret's position.</summary>
    TextPosition End { get; }

    /// <summary>Whichever of <see cref="Start"/>/<see cref="End"/> comes first in document order.</summary>
    TextPosition NormalizedStart { get; }

    /// <summary>Whichever of <see cref="Start"/>/<see cref="End"/> comes last in document order.</summary>
    TextPosition NormalizedEnd { get; }

    /// <summary>
    /// Sets the selection to span the given range.
    /// </summary>
    /// <param name="start">The anchor position.</param>
    /// <param name="end">The extending position.</param>
    void SetSelection(TextPosition start, TextPosition end);

    /// <summary>Clears the current selection, if any.</summary>
    void ClearSelection();
}