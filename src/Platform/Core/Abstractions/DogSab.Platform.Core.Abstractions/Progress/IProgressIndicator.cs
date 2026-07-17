namespace DogSab.Platform.Core.Abstractions.Progress;

/// <summary>Represents the observable and cancellable state of a long-running operation.</summary>
public interface IProgressIndicator
{
    /// <summary>Completion fraction between 0.0 and 1.0. Ignored when <see cref="IsIndeterminate"/> is true.</summary>
    double Fraction { get; set; }

    /// <summary>Primary status text describing the current step of the operation.</summary>
    string Text { get; set; }

    /// <summary>Secondary, more detailed status text shown alongside <see cref="Text"/>.</summary>
    string SecondaryText { get; set; }

    /// <summary>Indicates whether the operation's total duration/steps are unknown, showing a busy indicator instead of a fraction.</summary>
    bool IsIndeterminate { get; set; }

    /// <summary>Indicates whether cancellation has been requested for this operation.</summary>
    bool IsCanceled { get; }

    /// <summary>Requests cancellation of the operation.</summary>
    void Cancel();

    /// <summary>Throws an operation-canceled exception if <see cref="IsCanceled"/> is true; call periodically from long-running work.</summary>
    void CheckCanceled();

    /// <summary>Raised when the operation is canceled.</summary>
    event EventHandler? Canceled;
}