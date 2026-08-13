using DogSab.Platform.Indexing.Abstractions.Index;

namespace DogSab.Platform.Indexing.Abstractions.Dumb;

/// <summary>
/// Reports the platform's current <see cref="DumbModeState"/> and lets
/// callers wait for indexing to finish before running index-dependent logic.
/// Any feature that queries an <see cref="IIndex{TKey,TValue}"/> and
/// requires complete results (as opposed to a best-effort, progressively
/// improving suggestion list, which may tolerate dumb mode) should check or
/// wait on this service first.
/// </summary>
public interface IDumbService
{
    /// <summary>The platform's current indexing state.</summary>
    DumbModeState CurrentState { get; }

    /// <summary>Convenience shorthand for <c>CurrentState == DumbModeState.Dumb</c>.</summary>
    bool IsDumb { get; }

    /// <summary>
    /// Returns a task that completes once the platform transitions to
    /// <see cref="DumbModeState.Smart"/>. Completes immediately if already smart.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the wait.</param>
    /// <returns>A task that completes when the platform becomes smart.</returns>
    Task WaitForSmartModeAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Runs an action only once the platform is smart, deferring it (without
    /// blocking the calling thread) if currently dumb. Useful for UI actions
    /// that should quietly wait their turn rather than either blocking the
    /// UI thread or running against incomplete indexes.
    /// </summary>
    /// <param name="action">The action to run once smart mode is reached.</param>
    /// <param name="cancellationToken">Token used to cancel the wait.</param>
    /// <returns>A task that completes once <paramref name="action"/> has run.</returns>
    Task RunWhenSmartAsync(System.Action action, CancellationToken cancellationToken);
}