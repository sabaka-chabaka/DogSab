using Avalonia;
using Avalonia.Media;
using DogSab.Platform.Editor.Abstractions.Caret;

namespace DogSab.Platform.Editor.Ui.Rendering;

/// <summary>
/// Renders a blinking caret line at each active caret position, supporting
/// multiple simultaneous carets for multi-cursor editing.
/// Blinking is driven externally (see <see cref="EditorView"/>'s dispatcher
/// timer) by toggling <see cref="IsVisiblePhase"/> — this renderer itself
/// holds no timer of its own, keeping it a pure, testable rendering
/// function of its current inputs.
/// </summary>
public sealed class CaretRenderer
{
    /// <summary>
    /// The width, in pixels, of a single character column at the editor's
    /// current font size — used to convert a caret's column into a
    /// horizontal pixel offset. Supplied externally rather than computed
    /// here, since it depends on the same typeface/font size
    /// <see cref="EditorTextRenderer"/> already manages.
    /// </summary>
    public double CharacterWidth { get; set; }

    /// <summary>
    /// Whether the caret should currently be drawn, toggled periodically by
    /// the owning view to produce a blinking effect.
    /// </summary>
    public bool IsVisiblePhase { get; set; } = true;

    /// <summary>
    /// Renders every active caret position onto the given drawing context.
    /// </summary>
    /// <param name="context">
    /// The drawing context to render onto.
    /// </param>
    /// <param name="caretModel">
    /// The caret model whose active positions should be rendered.
    /// </param>
    /// <param name="lineHeight">
    /// The pixel height of a single rendered line, used to convert a
    /// caret's line number into a vertical pixel offset.
    /// </param>
    /// <param name="caretColorHex">
    /// The color to render carets in, as a hex string.
    /// </param>
    public void Render(DrawingContext context, ICaretModel caretModel, double lineHeight, string caretColorHex)
    {
        if (!IsVisiblePhase)
        {
            return;
        }

        var brush = Brush.Parse(caretColorHex);
        var pen = new Pen(brush, thickness: 1.5);

        foreach (var position in caretModel.AllCarets)
        {
            var x = position.Column * CharacterWidth;
            var y = position.Line * lineHeight;

            context.DrawLine(pen, new Point(x, y), new Point(x, y + lineHeight));
        }
    }
}