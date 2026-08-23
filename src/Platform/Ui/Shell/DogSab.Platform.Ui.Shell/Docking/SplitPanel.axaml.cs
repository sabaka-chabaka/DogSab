using Avalonia;
using Avalonia.Controls;

namespace DogSab.Platform.Ui.Shell.Docking;

/// <summary>
/// A resizable two-pane split, used to lay out a docked tool window panel
/// alongside the main editor/content area.
/// </summary>
public partial class SplitPanel : UserControl
{
    /// <summary>Styled property for <see cref="Orientation"/>.</summary>
    public static readonly StyledProperty<Avalonia.Layout.Orientation> OrientationProperty =
        AvaloniaProperty.Register<SplitPanel, Avalonia.Layout.Orientation>(nameof(Orientation));

    /// <summary>Whether the split divides its content horizontally or vertically.</summary>
    public Avalonia.Layout.Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public SplitPanel()
    {
        InitializeComponent();
    }

    /// <summary>Sets the content shown in the first (left/top) pane.</summary>
    /// <param name="content">The content to display.</param>
    public void SetFirstContent(object content)
    {
        FirstContent.Content = content;
    }

    /// <summary>Sets the content shown in the second (right/bottom) pane.</summary>
    /// <param name="content">The content to display.</param>
    public void SetSecondContent(object content)
    {
        SecondContent.Content = content;
    }
}