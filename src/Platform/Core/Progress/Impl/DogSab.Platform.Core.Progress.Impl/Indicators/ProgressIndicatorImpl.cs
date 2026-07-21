using DogSab.Platform.Core.Abstractions.Progress;

namespace DogSab.Platform.Core.Progress.Impl.Indicators;

/// <summary>
/// Default implementation of <see cref="IProgressIndicator"/>.
/// Purely a thread-safe state holder — it does not render anything itself;
/// UI components (e.g. a progress bar widget) are expected to subscribe to
/// its property changes and render accordingly. See the platform's UI layer
/// for how state changes here get reflected on screen.
/// </summary>
public sealed class ProgressIndicatorImpl : IProgressIndicator
{
    /// <summary>Guards mutation of the mutable fields below, since progress may be reported from any thread.</summary>
    private readonly object _lock = new();

    /// <summary>Backing field for <see cref="Fraction"/>.</summary>
    private double _fraction;

    /// <summary>Backing field for <see cref="Text"/>.</summary>
    private string _text = string.Empty;

    /// <summary>Backing field for <see cref="SecondaryText"/>.</summary>
    private string _secondaryText = string.Empty;

    /// <summary>Backing field for <see cref="IsIndeterminate"/>.</summary>
    private bool _isIndeterminate;

    /// <summary>Set to 1 once cancellation has been requested; used via <see cref="Interlocked"/> for lock-free reads.</summary>
    private int _isCanceledFlag;

    /// <inheritdoc />
    public double Fraction
    {
        get { lock (_lock) { return _fraction; } }
        set
        {
            lock (_lock) { _fraction = Math.Clamp(value, 0.0, 1.0); }
        }
    }

    /// <inheritdoc />
    public string Text
    {
        get { lock (_lock) { return _text; } }
        set { lock (_lock) { _text = value ?? string.Empty; } }
    }

    /// <inheritdoc />
    public string SecondaryText
    {
        get { lock (_lock) { return _secondaryText; } }
        set { lock (_lock) { _secondaryText = value ?? string.Empty; } }
    }

    /// <inheritdoc />
    public bool IsIndeterminate
    {
        get { lock (_lock) { return _isIndeterminate; } }
        set { lock (_lock) { _isIndeterminate = value; } }
    }

    /// <inheritdoc />
    public bool IsCanceled => Interlocked.CompareExchange(ref _isCanceledFlag, 0, 0) != 0;

    /// <inheritdoc />
    public event EventHandler? Canceled;

    /// <inheritdoc />
    public void Cancel()
    {
        if (Interlocked.Exchange(ref _isCanceledFlag, 1) != 0)
        {
            return; // already canceled; do not raise the event twice
        }

        Canceled?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    public void CheckCanceled()
    {
        if (IsCanceled)
        {
            throw new OperationCanceledException("The operation was canceled via its progress indicator.");
        }
    }
}