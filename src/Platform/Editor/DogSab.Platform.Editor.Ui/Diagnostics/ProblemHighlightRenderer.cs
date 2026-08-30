using Avalonia;
using Avalonia.Media;
using DogSab.Platform.Editor.Abstractions.Inspections;

namespace DogSab.Platform.Editor.Ui.Diagnostics;

/// <summary>
/// Renders squiggly underlines beneath text ranges reported as
/// <see cref="Problem"/> instances by <see cref="Editor.Inspections.InspectionCoordinator"/>.
/// Color is chosen from <see cref="Problem.Severity"/> rather than any
/// per-problem customization, since severity-based coloring (red for
/// errors, yellow for warnings) is the universally expected convention
/// across code editors, and individual inspections have no legitimate
/// reason to pick their own arbitrary colors outside that convention.
/// </summary>
public sealed class ProblemHighlightRenderer
{
    /// <summary>
    /// The height, in pixels, of a single squiggle "tooth" — the up/down
    /// zigzag segment repeated across a problem's underlined width.
    /// </summary>
    private const double SquiggleToothHeight = 2.0;

    /// <summary>
    /// The horizontal width, in pixels, of a single squiggle tooth.
    /// </summary>
    private const double SquiggleToothWidth = 4.0;

    /// <summary>
    /// Renders every problem's squiggly underline that falls within a
    /// single rendered line.
    /// </summary>
    /// <param name="context">
    /// The drawing context to render onto.
    /// </param>
    /// <param name="problemsForLine">
    /// The problems whose ranges fall (fully or partially) within this
    /// line, already filtered by the caller to just this line's range,
    /// paired with their horizontal start/end pixel positions on this
    /// line.
    /// </param>
    /// <param name="lineBaselineY">
    /// The vertical pixel position of the text baseline for this line,
    /// used to position each squiggle just beneath the rendered characters.
    /// </param>
    public void RenderProblemsForLine(
        DrawingContext context,
        IReadOnlyList<(Problem Problem, double StartX, double EndX)> problemsForLine,
        double lineBaselineY)
    {
        foreach (var (problem, startX, endX) in problemsForLine)
        {
            var pen = new Pen(ResolveSeverityBrush(problem.Severity), thickness: 1);
            RenderSquiggle(context, pen, startX, endX, lineBaselineY);
        }
    }

    /// <summary>
    /// Draws a single zigzag squiggle line spanning the given horizontal
    /// range, at the given vertical position.
    /// </summary>
    /// <param name="context">
    /// The drawing context to render onto.
    /// </param>
    /// <param name="pen">
    /// The pen to draw the squiggle with, already colored for the
    /// problem's severity.
    /// </param>
    /// <param name="startX">
    /// The horizontal pixel position the squiggle starts at.
    /// </param>
    /// <param name="endX">
    /// The horizontal pixel position the squiggle ends at.
    /// </param>
    /// <param name="baselineY">
    /// The vertical pixel position the squiggle is centered around.
    /// </param>
    private void RenderSquiggle(DrawingContext context, Pen pen, double startX, double endX, double baselineY)
    {
        var currentX = startX;
        var goingUp = true;

        while (currentX < endX)
        {
            var nextX = System.Math.Min(currentX + SquiggleToothWidth, endX);
            var startY = baselineY + (goingUp ? SquiggleToothHeight : 0);
            var endY = baselineY + (goingUp ? 0 : SquiggleToothHeight);

            context.DrawLine(pen, new Point(currentX, startY), new Point(nextX, endY));

            currentX = nextX;
            goingUp = !goingUp;
        }
    }

    /// <summary>
    /// Resolves the conventional squiggle color for a problem's severity —
    /// red for errors, a muted yellow/orange for warnings, and a subtle
    /// gray-blue for informational suggestions.
    /// </summary>
    /// <param name="severity">
    /// The severity to resolve a color for.
    /// </param>
    /// <returns>
    /// The brush to draw the squiggle with.
    /// </returns>
    private static IBrush ResolveSeverityBrush(ProblemSeverity severity) => severity switch
    {
        ProblemSeverity.Error => Brushes.Red,
        ProblemSeverity.Warning => Brushes.Orange,
        ProblemSeverity.Info => Brushes.SteelBlue,
        _ => Brushes.Gray
    };
}