namespace DogSab.Platform.Core.Abstractions.Progress;

/// <summary>An immutable snapshot of an <see cref="IProgressIndicator"/>'s state at a point in time.</summary>
public readonly struct ProgressIndicatorState
{
    /// <summary>Completion fraction at the time of the snapshot.</summary>
    public double Fraction { get; }

    /// <summary>Status text at the time of the snapshot.</summary>
    public string Text { get; }

    /// <summary>Whether the operation was indeterminate at the time of the snapshot.</summary>
    public bool IsIndeterminate { get; }

    /// <summary>Whether cancellation had been requested at the time of the snapshot.</summary>
    public bool IsCanceled { get; }

    /// <summary>
    /// Creates a new immutable snapshot of a progress indicator's state.
    /// </summary>
    /// <param name="fraction">Completion fraction between 0.0 and 1.0.</param>
    /// <param name="text">Status text describing the current step.</param>
    /// <param name="isIndeterminate">Whether the operation's duration is unknown.</param>
    /// <param name="isCanceled">Whether cancellation has been requested.</param>
    public ProgressIndicatorState(double fraction, string text, bool isIndeterminate, bool isCanceled)
    {
        Fraction = fraction;
        Text = text;
        IsIndeterminate = isIndeterminate;
        IsCanceled = isCanceled;
    }
}