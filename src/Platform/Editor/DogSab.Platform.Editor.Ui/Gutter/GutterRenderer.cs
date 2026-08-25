using Avalonia;
using Avalonia.Media;

namespace DogSab.Platform.Editor.Ui.Gutter;

/// <summary>
/// Renders the left-hand gutter strip alongside the editor's text: line
/// numbers for now, with room reserved for breakpoint markers once the
/// Debugger module exists and folding indicators (see
/// <see cref="FoldingIndicatorRenderer"/>) drawn alongside it.
/// Kept as a narrow, single-purpose renderer — line numbers only — so the
/// gutter's total width can be composed from this renderer's fixed number
/// column plus whatever other gutter renderers (folding, breakpoints)
/// contribute their own widths, rather than one large renderer trying to
/// own every gutter concern at once.
/// </summary>
public sealed class GutterRenderer
{
    /// <summary>
    /// The typeface used for line numbers — matches the editor's own
    /// monospace typeface so digit widths align predictably, though line
    /// numbers do not need to align character-for-character with the code
    /// text itself.
    /// </summary>
    private readonly Typeface _typeface = new("Consolas, Cascadia Mono, monospace");

    /// <summary>
    /// The font size used for line numbers, kept slightly smaller than the
    /// main text to visually de-emphasize the gutter relative to the code.
    /// </summary>
    private readonly double _fontSize = 12;

    /// <summary>
    /// Computes the pixel width the line-number column needs to
    /// accommodate the largest line number in the document, so the gutter
    /// doesn't need to be resized as the user scrolls past increasing
    /// digit counts (e.g. from 99 to 100).
    /// </summary>
    /// <param name="totalLineCount">
    /// The total number of lines in the document.
    /// </param>
    /// <returns>
    /// The required pixel width for the line-number column, including a
    /// small margin.
    /// </returns>
    public double ComputeRequiredWidth(int totalLineCount)
    {
        var digitCount = totalLineCount.ToString().Length;
        var approximateCharacterWidth = _fontSize * 0.6;

        return digitCount * approximateCharacterWidth + 12;
    }

    /// <summary>
    /// Renders a single line number at the given vertical offset.
    /// </summary>
    /// <param name="context">
    /// The drawing context to render onto.
    /// </param>
    /// <param name="lineNumberOneBased">
    /// The line number to display, using conventional one-based numbering
    /// for user display even though the rest of the platform indexes lines
    /// from zero internally.
    /// </param>
    /// <param name="topOffset">
    /// The vertical pixel offset to render at.
    /// </param>
    /// <param name="columnWidth">
    /// The total width of the gutter's line-number column, used to
    /// right-align the number within it.
    /// </param>
    /// <param name="textColorHex">
    /// The color to render the number in, as a hex string.
    /// </param>
    public void RenderLineNumber(
        DrawingContext context,
        int lineNumberOneBased,
        double topOffset,
        double columnWidth,
        string textColorHex)
    {
        var text = lineNumberOneBased.ToString();

        var formattedText = new FormattedText(
            text,
            System.Globalization.CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            _typeface,
            _fontSize,
            Brush.Parse(textColorHex));

        var rightAlignedX = columnWidth - formattedText.Width - 6;

        context.DrawText(formattedText, new Point(rightAlignedX, topOffset));
    }
}