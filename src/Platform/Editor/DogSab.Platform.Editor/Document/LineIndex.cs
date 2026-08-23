using DogSab.Platform.Editor.Abstractions.Caret;

namespace DogSab.Platform.Editor.Document;

/// <summary>
/// Maintains a sorted array of line-start offsets for a document's text,
/// enabling O(log n) conversion between a flat character offset and its
/// (line, column) position — as opposed to an O(n) full-text rescan on every
/// lookup. Updated incrementally on each edit via <see cref="ApplyChange"/>:
/// only the affected region's line boundaries are recomputed, and every line
/// after the edit has its offset shifted by the edit's net length delta,
/// rather than rescanning the entire document on every keystroke.
/// </summary>
internal sealed class LineIndex
{
    /// <summary>The character offset each line starts at, in ascending order. Index 0 is always 0 (line 0 always starts at offset 0).</summary>
    private List<int> _lineStartOffsets = new() { 0 };

    /// <summary>
    /// Rebuilds the index from scratch for the given full text. Called once
    /// when a document is first created; subsequent changes use
    /// <see cref="ApplyChange"/> instead.
    /// </summary>
    /// <param name="fullText">The document's full current text.</param>
    public void Rebuild(string fullText)
    {
        _lineStartOffsets = ComputeLineStarts(fullText, 0, fullText.Length);
    }

    /// <summary>
    /// Incrementally updates the index after a text replacement, without
    /// rescanning the whole document.
    /// </summary>
    /// <param name="offset">The offset where the replaced range started.</param>
    /// <param name="oldLength">The length of the text that was replaced.</param>
    /// <param name="newText">The text that replaced it.</param>
    /// <param name="fullTextAfterChange">The document's full text after the change has been applied.</param>
    public void ApplyChange(int offset, int oldLength, string newText, string fullTextAfterChange)
    {
        // Find which line the edit starts on — everything before this line
        // is unaffected and its offsets stay exactly as they are.
        var startLine = FindLineIndex(offset);
        var lengthDelta = newText.Length - oldLength;

        // Find where, in the NEW text, the edit's effects end — one
        // newline past the last newline introduced or affected by the edit,
        // so we recompute line starts for exactly the region that changed
        // plus a small safety margin, not the whole file.
        var rescanStart = _lineStartOffsets[startLine];
        var rescanEndInNewText = Math.Min(fullTextAfterChange.Length, offset + newText.Length);

        var recomputedLines = ComputeLineStarts(fullTextAfterChange, rescanStart, rescanEndInNewText);

        // Lines strictly after the edited region keep their old relative
        // structure but must be shifted by the net length delta, since
        // everything after the edit moved in the text by that amount.
        var unaffectedTailStartLine = FindLineIndex(offset + oldLength) + 1;
        var shiftedTail = new List<int>();
        for (var i = unaffectedTailStartLine; i < _lineStartOffsets.Count; i++)
        {
            shiftedTail.Add(_lineStartOffsets[i] + lengthDelta);
        }

        var newIndex = new List<int>(_lineStartOffsets.GetRange(0, startLine));
        newIndex.AddRange(recomputedLines);
        newIndex.AddRange(shiftedTail);

        _lineStartOffsets = newIndex;
    }

    /// <summary>
    /// Converts a flat character offset into a full <see cref="TextPosition"/>
    /// carrying both the offset and its (line, column) representation.
    /// </summary>
    /// <param name="offset">The offset to convert.</param>
    /// <returns>The resulting text position.</returns>
    public TextPosition ToPosition(int offset)
    {
        var line = FindLineIndex(offset);
        var column = offset - _lineStartOffsets[line];
        return new TextPosition(offset, line, column);
    }

    /// <summary>
    /// Converts a (line, column) pair back into a flat offset.
    /// </summary>
    /// <param name="line">The zero-based line number.</param>
    /// <param name="column">The zero-based column number.</param>
    /// <returns>The corresponding flat offset.</returns>
    public int ToOffset(int line, int column)
    {
        return _lineStartOffsets[line] + column;
    }

    /// <summary>
    /// Finds which line contains a given offset via binary search over the
    /// sorted <see cref="_lineStartOffsets"/> array — the line whose start
    /// offset is the greatest one not exceeding <paramref name="offset"/>.
    /// </summary>
    /// <param name="offset">The offset to locate.</param>
    /// <returns>The zero-based line index containing <paramref name="offset"/>.</returns>
    private int FindLineIndex(int offset)
    {
        var index = _lineStartOffsets.BinarySearch(offset);

        // List<T>.BinarySearch returns the exact index if found, or the
        // bitwise complement of the index of the first element GREATER than
        // the search value if not found — so ~index - 1 gives the last
        // element less than or equal to offset, which is the line we want.
        return index >= 0 ? index : Math.Max(0, ~index - 1);
    }

    /// <summary>
    /// Scans a range of text and returns the offset of the start of each
    /// line found within it (each position immediately after a '\n').
    /// </summary>
    /// <param name="text">The full text being scanned.</param>
    /// <param name="scanStart">The offset to start scanning from.</param>
    /// <param name="scanEnd">The offset to stop scanning at.</param>
    /// <returns>Line start offsets found in the range, including <paramref name="scanStart"/> itself as the first entry.</returns>
    private static List<int> ComputeLineStarts(string text, int scanStart, int scanEnd)
    {
        var starts = new List<int> { scanStart };

        for (var i = scanStart; i < scanEnd && i < text.Length; i++)
        {
            if (text[i] == '\n')
            {
                starts.Add(i + 1);
            }
        }

        return starts;
    }
}