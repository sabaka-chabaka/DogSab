using Avalonia.Controls;
using Avalonia.Threading;
using DogSab.Platform.Core.Abstractions.Progress;

namespace DogSab.Platform.Ui.Progress;

public partial class ProgressBarWidget : UserControl
{
    private readonly DispatcherTimer _refreshTimer;
    private IProgressIndicator? _indicator;

    public ProgressBarWidget()
    {
        InitializeComponent();

        _refreshTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(200), DispatcherPriority.Background, (_, _) => Refresh());
        _refreshTimer.Start();
    }

    /// <summary>
    /// Binds this widget to display a specific indicator's live state.
    /// Refresh happens automatically on an internal timer — callers no
    /// longer need to invoke <see cref="Refresh"/> manually.
    /// </summary>
    /// <param name="indicator">The indicator to display, or <c>null</c> to detach.</param>
    public void Attach(IProgressIndicator? indicator)
    {
        _indicator = indicator;
        Refresh();
    }

    private void Refresh()
    {
        if (_indicator is null)
        {
            IsVisible = false;
            return;
        }

        IsVisible = true;
        ProgressText.Text = _indicator.Text;
        Bar.IsIndeterminate = _indicator.IsIndeterminate;

        if (!_indicator.IsIndeterminate)
        {
            Bar.Value = _indicator.Fraction * 100;
        }
    }
}