using DogSab.Platform.Indexing.Abstractions.Dumb;

namespace DogSab.Platform.Indexing.Dumb;

/// <summary>
/// Default implementation of <see cref="IDumbService"/>. Holds the current
/// <see cref="DumbModeState"/> and a <see cref="TaskCompletionSource"/> that
/// completes when the platform transitions to <see cref="DumbModeState.Smart"/>,
/// letting <see cref="WaitForSmartModeAsync"/> await it without polling. State
/// transitions are driven externally by <c>Building.IndexBuildScheduler</c>,
/// which calls <see cref="EnterDumbMode"/> when a batch of indexing work
/// starts and <see cref="EnterSmartMode"/> when it finishes.
/// </summary>
public sealed class DumbServiceImpl : IDumbService
{
    /// <summary>Guards transitions between states and the completion source, since multiple threads may query or transition state concurrently.</summary>
    private readonly object _lock = new();

    /// <summary>Completed and replaced with a fresh instance each time smart mode is reached, so waiters queued during a later dumb period get a new signal.</summary>
    private TaskCompletionSource _smartModeSignal = CreateCompletedSignal();

    /// <inheritdoc />
    public DumbModeState CurrentState { get; private set; } = DumbModeState.Smart;

    /// <inheritdoc />
    public bool IsDumb => CurrentState == DumbModeState.Dumb;

    /// <summary>
    /// Transitions the platform into dumb mode, replacing the smart-mode
    /// signal with a fresh, incomplete one so any concurrent or future
    /// waiters correctly block until the next smart transition.
    /// </summary>
    public void EnterDumbMode()
    {
        lock (_lock)
        {
            if (CurrentState == DumbModeState.Dumb)
            {
                return;
            }

            CurrentState = DumbModeState.Dumb;
            _smartModeSignal = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        }
    }

    /// <summary>
    /// Transitions the platform into smart mode, completing the current
    /// smart-mode signal so any waiters proceed.
    /// </summary>
    public void EnterSmartMode()
    {
        TaskCompletionSource signalToComplete;

        lock (_lock)
        {
            if (CurrentState == DumbModeState.Smart)
            {
                return;
            }

            CurrentState = DumbModeState.Smart;
            signalToComplete = _smartModeSignal;
        }

        signalToComplete.TrySetResult();
    }

    /// <inheritdoc />
    public async Task WaitForSmartModeAsync(CancellationToken cancellationToken)
    {
        Task signalTask;

        lock (_lock)
        {
            if (CurrentState == DumbModeState.Smart)
            {
                return;
            }

            signalTask = _smartModeSignal.Task;
        }

        await using var registration = cancellationToken.Register(static state =>
        {
            ((TaskCompletionSource)state!).TrySetCanceled();
        }, _smartModeSignal);

        await signalTask.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task RunWhenSmartAsync(Action action, CancellationToken cancellationToken)
    {
        await WaitForSmartModeAsync(cancellationToken).ConfigureAwait(false);
        action();
    }

    /// <summary>Creates an already-completed signal, used as the initial state since the platform starts smart.</summary>
    private static TaskCompletionSource CreateCompletedSignal()
    {
        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        tcs.SetResult();
        return tcs;
    }
}