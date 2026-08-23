using DogSab.Platform.Editor.Abstractions.Caret;
using DogSab.Platform.Editor.Abstractions.Selection;

namespace DogSab.Platform.Editor.Selection;

/// <summary>
/// Default implementation of <see cref="ISelectionModel"/>.
/// A selection with equal start and end position is treated as having
/// no selection at all, matching the contract's stated behavior that a
/// zero-length selection does not count as a real selection.
/// </summary>
public sealed class SelectionModelImpl : ISelectionModel
{
    /// <summary>
    /// The current anchor position, set when the selection was started.
    /// Defaults to the zero position until a real selection is made.
    /// </summary>
    private TextPosition _start;

    /// <summary>
    /// The current extending position, typically following the caret.
    /// Defaults to the zero position until a real selection is made.
    /// </summary>
    private TextPosition _end;

    /// <inheritdoc />
    public bool HasSelection => _start.Offset != _end.Offset;

    /// <inheritdoc />
    public TextPosition Start => _start;

    /// <inheritdoc />
    public TextPosition End => _end;

    /// <inheritdoc />
    public TextPosition NormalizedStart => _start.Offset <= _end.Offset ? _start : _end;

    /// <inheritdoc />
    public TextPosition NormalizedEnd => _start.Offset <= _end.Offset ? _end : _start;

    /// <inheritdoc />
    public void SetSelection(TextPosition start, TextPosition end)
    {
        _start = start;
        _end = end;
    }

    /// <inheritdoc />
    public void ClearSelection()
    {
        _start = _end;
    }
}