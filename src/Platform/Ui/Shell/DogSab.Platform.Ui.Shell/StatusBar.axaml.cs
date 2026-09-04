using Avalonia.Controls;

namespace DogSab.Platform.Ui.Shell;

/// <summary>
/// The bottom status bar: general status text on the left, a panel on the
/// right for widgets contributed by other modules (e.g. Ui.Progress).
/// </summary>
public partial class StatusBar : UserControl
{
    public StatusBar()
    {
        InitializeComponent();
        SetStatusText("Ready");
    }

    /// <summary>Sets the general status text shown on the left.</summary>
    /// <param name="text">The text to display.</param>
    public void SetStatusText(string text)
    {
        StatusText.Text = text;
    }

    /// <summary>Adds a widget to the right side of the status bar.</summary>
    /// <param name="widget">The control to add.</param>
    public void AddRightWidget(Control widget)
    {
        RightItemsPanel.Children.Add(widget);
    }
}