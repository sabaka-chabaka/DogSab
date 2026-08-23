using DogSab.Platform.Editor.Abstractions.Caret;
using DogSab.Platform.Editor.Document;

namespace DogSab.Platform.Editor.Caret;

/// <summary>
/// Default implementation of <see cref="ICaretModel"/>.
/// Delegates offset-to-position conversion to the owning document's
/// internal <see cref="LineIndex"/>, so caret positions always reflect the
/// document's current line structure rather than a snapshot taken when the
/// caret was last moved.
/// </summary>
public sealed class CaretModelImpl : ICaretModel
{
    /// <summary>
    /// The document this caret model belongs to, used to resolve
    /// (line, column) coordinates from flat offsets via its line index.
    /// </summary>
    private readonly DocumentImpl _document;

    /// <summary>
    /// The offsets of every currently active caret, in ascending document
    /// order.
    /// The first entry is always the primary caret.
    /// </summary>
    private readonly List<int> _caretOffsets = new() { 0 };

    /// <summary>
    /// Creates a new caret model for a document, with a single primary
    /// caret initially positioned at the start of the document.
    /// </summary>
    /// <param name="document">
    /// The document this caret model tracks positions within.
    /// </param>
    public CaretModelImpl(DocumentImpl document)
    {
        _document = document;
    }

    /// <inheritdoc />
    public TextPosition PrimaryCaret => ResolvePosition(_caretOffsets[0]);

    /// <inheritdoc />
    public IReadOnlyList<TextPosition> AllCarets
    {
        get
        {
            var positions = new List<TextPosition>(_caretOffsets.Count);

            foreach (var offset in _caretOffsets)
            {
                positions.Add(ResolvePosition(offset));
            }

            return positions;
        }
    }

    /// <inheritdoc />
    public void MoveTo(TextPosition position)
    {
        _caretOffsets.Clear();
        _caretOffsets.Add(ClampOffset(position.Offset));
    }

    /// <inheritdoc />
    public void AddCaret(TextPosition position)
    {
        var clampedOffset = ClampOffset(position.Offset);

        if (_caretOffsets.Contains(clampedOffset))
        {
            return;
        }

        _caretOffsets.Add(clampedOffset);
        _caretOffsets.Sort();
    }

    /// <inheritdoc />
    public void CollapseToPrimary()
    {
        if (_caretOffsets.Count <= 1)
        {
            return;
        }

        var primaryOffset = _caretOffsets[0];
        _caretOffsets.Clear();
        _caretOffsets.Add(primaryOffset);
    }

    /// <summary>
    /// Converts a flat offset into a full <see cref="TextPosition"/> using
    /// the owning document's line index.
    /// This keeps the (line, column) portion of every returned position
    /// consistent with the document's actual current content, even if the
    /// document changed since the caret was last explicitly moved.
    /// </summary>
    /// <param name="offset">
    /// The flat character offset to resolve.
    /// </param>
    /// <returns>
    /// The resolved text position.
    /// </returns>
    private TextPosition ResolvePosition(int offset)
    {
        return _document.ResolvePosition(offset);
    }

    /// <summary>
    /// Clamps an offset to the valid range of the document's current
    /// length, so a caret can never be positioned before the start or
    /// after the end of the document — e.g. after text was deleted out
    /// from under an existing caret position.
    /// </summary>
    /// <param name="offset">
    /// The offset to clamp.
    /// </param>
    /// <returns>
    /// The clamped offset, guaranteed to be within
    /// <c>[0, document.Length]</c>.
    /// </returns>
    private int ClampOffset(int offset)
    {
        return Math.Clamp(offset, 0, _document.Length);
    }
}