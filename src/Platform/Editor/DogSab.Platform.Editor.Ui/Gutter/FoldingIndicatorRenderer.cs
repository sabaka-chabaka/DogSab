using Avalonia;
using Avalonia.Media;
using DogSab.Platform.Editor.Abstractions.Folding;

namespace DogSab.Platform.Editor.Ui.Gutter;

/// <summary>
/// Renders the small [+]/[-] fold/unfold indicators in the gutter next to
/// lines where a <see cref="IFoldingRegion"/> begins.
/// Only draws the indicator itself here — the interactive collapsed/expanded
/// state and the actual hiding of folded text is tracked by
/// <see cref="EditorView"/>, which owns the mapping from region to current
/// fold state; this renderer is a pure function of "is this region
/// currently collapsed" passed in by the caller.
/// </summary>
public sealed class FoldingIndicatorRenderer
{
    /// <summary>
    /// The fixed pixel width reserved for a single folding indicator glyph.
    /// </summary>
    public const double IndicatorWidth = 14;

    /// <summary>
    /// The pen used to draw the indicator's box outline.
    /// </summary>
    private readonly Pen _outlinePen = new(Brushes.Gray, thickness: 1);

    /// <summary>
    /// Renders a single folding indicator glyph — a small box containing
    /// either a plus (collapsed) or minus (expanded) sign — at the given
    /// position.
    /// </summary>
    /// <param name="context">
    /// The drawing context to render onto.
    /// </param>
    /// <param name="region">
    /// The folding region this indicator represents, used only to decide
    /// whether it should be drawn at all — every foldable line gets an
    /// indicator regardless of the region's own
    /// <see cref="IFoldingRegion.CollapsedByDefault"/>, since that property
    /// only affects the initial state when a file is first opened, not
    /// whether an indicator is shown at all.
    /// </param>
    /// <param name="isCurrentlyCollapsed">
    /// Whether this specific region is currently collapsed, as tracked by
    /// the owning <see cref="EditorView"/> — determines whether a plus or
    /// minus sign is drawn.
    /// </param>
    /// <param name="topOffset">
    /// The vertical pixel offset to render at.
    /// </param>
    /// <param name="leftOffset">
    /// The horizontal pixel offset to render at, within the gutter's
    /// folding column.
    /// </param>
    public void RenderIndicator(
        DrawingContext context,
        IFoldingRegion region,
        bool isCurrentlyCollapsed,
        double topOffset,
        double leftOffset)
    {
        var boxSize = 10.0;
        var boxRect = new Rect(leftOffset, topOffset, boxSize, boxSize);

        context.DrawRectangle(Brushes.Transparent, _outlinePen, boxRect);

        // Draw the horizontal line of the +/- glyph, always present.
        var midY = topOffset + boxSize / 2;
        context.DrawLine(_outlinePen, new Point(leftOffset + 2, midY), new Point(leftOffset + boxSize - 2, midY));

        if (isCurrentlyCollapsed)
        {
            // Add the vertical stroke to turn the minus into a plus,
            // signaling that clicking will expand this collapsed region.
            var midX = leftOffset + boxSize / 2;
            context.DrawLine(_outlinePen, new Point(midX, topOffset + 2), new Point(midX, topOffset + boxSize - 2));
        }
    }
}