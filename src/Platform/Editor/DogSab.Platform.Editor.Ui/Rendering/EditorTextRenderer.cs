using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using DogSab.Platform.Editor.Highlighting;
using DogSab.Platform.Ui.Themes;

namespace DogSab.Platform.Editor.Ui.Rendering;

/// <summary>
/// Renders a document's text line by line onto an Avalonia
/// <see cref="DrawingContext"/>, applying color per
/// <see cref="HighlightSpan"/> where one covers a given character range.
/// A custom renderer rather than a built-in text control (like
/// <see cref="TextBox"/>), since a code editor needs full control over
/// per-character coloring, gutter alignment, and virtualized rendering of
/// only the currently visible line range — none of which a general-purpose
/// text box control is designed to support efficiently for large files.
/// </summary>
public sealed class EditorTextRenderer
{
    /// <summary>
    /// The typeface used to render all editor text. A fixed-width
    /// (monospace) font is required for column-based caret positioning and
    /// gutter alignment to line up visually with the text.
    /// </summary>
    private readonly Typeface _typeface = new("Consolas, Cascadia Mono, monospace");

    /// <summary>
    /// The font size, in device-independent pixels, used to render text.
    /// </summary>
    private readonly double _fontSize = 14;

    /// <summary>
    /// The height of a single rendered line, derived from the font size
    /// with a small amount of extra line spacing.
    /// </summary>
    public double LineHeight => _fontSize * 1.4;

    /// <summary>
    /// Renders a single line of text at the given vertical offset,
    /// applying colors from any highlight spans that fall within this line's range.
    /// </summary>
    /// <param name="context">
    /// The drawing context to render onto.
    /// </param>
    /// <param name="lineText">
    /// The text content of this single line, without a trailing newline.
    /// </param>
    /// <param name="lineStartOffsetInDocument">
    /// The document-wide character offset this line begins at, used to
    /// translate document-relative highlight span offsets into
    /// line-relative ones.
    /// </param>
    /// <param name="highlightSpansForLine">
    /// The highlight spans that fall (fully or partially) within this line,
    /// already filtered by the caller to just this line's range.
    /// </param>
    /// <param name="topOffset">
    /// The vertical pixel offset to render this line at.
    /// </param>
    /// <param name="theme">
    /// The current theme, used to resolve each highlight span's category
    /// into an actual display color.
    /// </param>
    public void RenderLine(
        DrawingContext context,
        string lineText,
        int lineStartOffsetInDocument,
        IReadOnlyList<HighlightSpan> highlightSpansForLine,
        double topOffset,
        ITheme theme)
    {
        if (highlightSpansForLine.Count == 0)
        {
            RenderPlainText(context, lineText, topOffset, theme.ForegroundColor);
            return;
        }

        RenderHighlightedText(context, lineText, lineStartOffsetInDocument, highlightSpansForLine, topOffset, theme);
    }

    /// <summary>
    /// Renders a line with no highlighting applied, using the theme's
    /// default foreground color for the entire line.
    /// </summary>
    /// <param name="context">
    /// The drawing context to render onto.
    /// </param>
    /// <param name="lineText">
    /// The text content of the line.
    /// </param>
    /// <param name="topOffset">
    /// The vertical pixel offset to render at.
    /// </param>
    /// <param name="foregroundColorHex">
    /// The default foreground color, as a hex string.
    /// </param>
    private void RenderPlainText(DrawingContext context, string lineText, double topOffset, string foregroundColorHex)
    {
        var formattedText = new FormattedText(
            lineText,
            System.Globalization.CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            _typeface,
            _fontSize,
            Brush.Parse(foregroundColorHex));

        context.DrawText(formattedText, new Point(0, topOffset));
    }

    /// <summary>
    /// Renders a line by splitting it into colored runs wherever a
    /// highlight span's category changes, drawing each run separately at
    /// its correct horizontal offset.
    /// </summary>
    /// <param name="context">
    /// The drawing context to render onto.
    /// </param>
    /// <param name="lineText">
    /// The text content of the line.
    /// </param>
    /// <param name="lineStartOffsetInDocument">
    /// The document-wide offset this line begins at.
    /// </param>
    /// <param name="highlightSpansForLine">
    /// The highlight spans covering portions of this line.
    /// </param>
    /// <param name="topOffset">
    /// The vertical pixel offset to render at.
    /// </param>
    /// <param name="theme">
    /// The current theme, used to resolve colors — currently only
    /// providing a fallback foreground; per-category color resolution is a
    /// known simplification noted below.
    /// </param>
    private void RenderHighlightedText(
        DrawingContext context,
        string lineText,
        int lineStartOffsetInDocument,
        IReadOnlyList<HighlightSpan> highlightSpansForLine,
        double topOffset,
        ITheme theme)
    {
        var cursorX = 0.0;
        var lastRenderedEnd = 0;

        foreach (var span in highlightSpansForLine)
        {
            var lineRelativeStart = Math.Max(0, span.StartOffset - lineStartOffsetInDocument);
            var lineRelativeEnd = Math.Min(lineText.Length, lineRelativeStart + span.Length);

            if (lineRelativeStart > lastRenderedEnd)
            {
                cursorX = RenderRun(context, lineText[lastRenderedEnd..lineRelativeStart], cursorX, topOffset, theme.ForegroundColor);
            }

            // NOTE: category-to-color resolution is a known simplification —
            // this renders every highlighted run in the theme's default
            // foreground rather than actually looking up a per-category
            // color, since ITheme currently only exposes a handful of fixed
            // named colors (see Ui.Themes.ITheme), not an open-ended
            // category-to-color map. A real implementation needs ITheme
            // extended with such a map before per-token-kind coloring works.
            var runText = lineText[lineRelativeStart..lineRelativeEnd];
            var categoryColor = theme.ResolveCategoryColor(span.Category);
            cursorX = RenderRun(context, runText, cursorX, topOffset, categoryColor);

            lastRenderedEnd = lineRelativeEnd;
        }

        if (lastRenderedEnd < lineText.Length)
        {
            RenderRun(context, lineText[lastRenderedEnd..], cursorX, topOffset, theme.ForegroundColor);
        }
    }

    /// <summary>
    /// Renders a single contiguous run of same-colored text at a given
    /// horizontal offset, returning the horizontal offset immediately after
    /// it for the next run to continue from.
    /// </summary>
    /// <param name="context">
    /// The drawing context to render onto.
    /// </param>
    /// <param name="runText">
    /// The text of this run.
    /// </param>
    /// <param name="startX">
    /// The horizontal pixel offset to start rendering at.
    /// </param>
    /// <param name="topOffset">
    /// The vertical pixel offset to render at.
    /// </param>
    /// <param name="colorHex">
    /// The color to render this run in, as a hex string.
    /// </param>
    /// <returns>
    /// The horizontal pixel offset immediately after this run's rendered width.
    /// </returns>
    private double RenderRun(DrawingContext context, string runText, double startX, double topOffset, string colorHex)
    {
        if (runText.Length == 0)
        {
            return startX;
        }

        var formattedText = new FormattedText(
            runText,
            System.Globalization.CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            _typeface,
            _fontSize,
            Brush.Parse(colorHex));

        context.DrawText(formattedText, new Point(startX, topOffset));

        return startX + formattedText.Width;
    }
}