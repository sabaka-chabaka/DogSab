using System;

namespace DogSab.Platform.Core.Abstractions.Threading;

/// <summary>
/// Manages read/write locking over the platform's shared model
/// (analogous to ReadAction/WriteAction in IntelliJ Platform).
/// Multiple readers are allowed concurrently, a writer requires exclusivity.
/// </summary>
public interface IReadWriteActionManager
{
    /// <summary>
    /// Runs a function under a shared read lock and returns its result.
    /// </summary>
    /// <typeparam name="T">The type of value produced.</typeparam>
    /// <param name="action">The function to execute under the read lock.</param>
    /// <returns>The value returned by <paramref name="action"/>.</returns>
    T RunReadAction<T>(Func<T> action);

    /// <summary>
    /// Runs an action under a shared read lock.
    /// </summary>
    /// <param name="action">The action to execute under the read lock.</param>
    void RunReadAction(Action action);

    /// <summary>
    /// Runs an action under an exclusive write lock.
    /// </summary>
    /// <param name="action">The action to execute under the write lock.</param>
    void RunWriteAction(Action action);

    /// <summary>
    /// Runs a function under an exclusive write lock and returns its result.
    /// </summary>
    /// <typeparam name="T">The type of value produced.</typeparam>
    /// <param name="action">The function to execute under the write lock.</param>
    /// <returns>The value returned by <paramref name="action"/>.</returns>
    T RunWriteAction<T>(Func<T> action);

    /// <summary>
    /// Attempts to acquire the read lock and run an action within the given timeout.
    /// </summary>
    /// <param name="action">The action to execute under the read lock.</param>
    /// <param name="timeout">The maximum time to wait for the lock to become available.</param>
    /// <returns><c>true</c> if the lock was acquired and the action executed; otherwise <c>false</c>.</returns>
    bool TryRunReadAction(Action action, TimeSpan timeout);

    /// <summary>Indicates whether the calling code is currently executing inside a read action.</summary>
    bool IsInsideReadAction { get; }

    /// <summary>Indicates whether the calling code is currently executing inside a write action.</summary>
    bool IsInsideWriteAction { get; }
}