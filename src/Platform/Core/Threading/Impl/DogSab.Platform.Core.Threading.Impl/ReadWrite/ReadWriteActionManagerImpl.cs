using DogSab.Platform.Core.Abstractions.Logging;
using DogSab.Platform.Core.Abstractions.Threading;

namespace DogSab.Platform.Core.Threading.Impl.ReadWrite;

/// <summary>
/// Default implementation of <see cref="IReadWriteActionManager"/>.
/// Backs the platform's shared-model locking with a single
/// <see cref="PlatformReaderWriterLock"/>: multiple read actions may run
/// concurrently, while a write action requires exclusive access and blocks
/// until all current readers have finished.
/// </summary>
public sealed class ReadWriteActionManagerImpl : IReadWriteActionManager
{
    /// <summary>The underlying lock guarding the platform's shared model.</summary>
    private readonly PlatformReaderWriterLock _platformLock;
    
    /// <summary>The underlying lock, exposed internally for diagnostics components like <see cref="Diagnostics.DeadlockDetector"/>.</summary>
    internal PlatformReaderWriterLock UnderlyingLock => _platformLock;

    /// <summary>
    /// Creates a new read/write action manager.
    /// </summary>
    /// <param name="loggerFactory">Factory used to obtain a logger for the underlying lock's diagnostics.</param>
    public ReadWriteActionManagerImpl(ILoggerFactory loggerFactory)
    {
        _platformLock = new PlatformReaderWriterLock(loggerFactory);
    }

    /// <inheritdoc />
    public bool IsInsideReadAction => _platformLock.IsReadLockHeldByCurrentThread;

    /// <inheritdoc />
    public bool IsInsideWriteAction => _platformLock.IsWriteLockHeldByCurrentThread;

    /// <summary>
    /// Runs a function under a shared read lock and returns its result.
    /// </summary>
    /// <typeparam name="T">The type of value produced.</typeparam>
    /// <param name="action">The function to execute under the read lock.</param>
    /// <returns>The value returned by <paramref name="action"/>.</returns>
    public T RunReadAction<T>(Func<T> action)
    {
        using var scope = new ReadActionScope(_platformLock);
        return action();
    }

    /// <summary>
    /// Runs an action under a shared read lock.
    /// </summary>
    /// <param name="action">The action to execute under the read lock.</param>
    public void RunReadAction(Action action)
    {
        using var scope = new ReadActionScope(_platformLock);
        action();
    }

    /// <summary>
    /// Runs an action under an exclusive write lock.
    /// </summary>
    /// <param name="action">The action to execute under the write lock.</param>
    public void RunWriteAction(Action action)
    {
        using var scope = new WriteActionScope(_platformLock);
        action();
    }

    /// <summary>
    /// Runs a function under an exclusive write lock and returns its result.
    /// </summary>
    /// <typeparam name="T">The type of value produced.</typeparam>
    /// <param name="action">The function to execute under the write lock.</param>
    /// <returns>The value returned by <paramref name="action"/>.</returns>
    public T RunWriteAction<T>(Func<T> action)
    {
        using var scope = new WriteActionScope(_platformLock);
        return action();
    }

    /// <summary>
    /// Attempts to acquire the read lock and run an action within the given timeout.
    /// </summary>
    /// <param name="action">The action to execute under the read lock.</param>
    /// <param name="timeout">The maximum time to wait for the lock to become available.</param>
    /// <returns><c>true</c> if the lock was acquired and the action executed; otherwise <c>false</c>.</returns>
    public bool TryRunReadAction(Action action, TimeSpan timeout)
    {
        if (!_platformLock.TryEnterReadLock(timeout))
        {
            return false;
        }

        try
        {
            action();
            return true;
        }
        finally
        {
            _platformLock.ExitReadLock();
        }
    }
}