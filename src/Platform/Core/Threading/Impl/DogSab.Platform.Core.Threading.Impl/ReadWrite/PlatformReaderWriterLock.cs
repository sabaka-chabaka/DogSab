using DogSab.Platform.Core.Abstractions.Logging;

namespace DogSab.Platform.Core.Threading.Impl.ReadWrite;

/// <summary>
/// Wraps <see cref="ReaderWriterLockSlim"/> with platform-specific diagnostics:
/// tracks which thread currently holds the write lock and for how long, so that
/// a stuck write action can be identified from logs instead of only observing
/// that the application has become unresponsive.
/// Multiple readers may hold the lock concurrently; a writer requires exclusive access
/// and blocks until all current readers have released it.
/// </summary>
public sealed class PlatformReaderWriterLock : IDisposable
{
    /// <summary>The underlying .NET reader/writer lock.</summary>
    private readonly ReaderWriterLockSlim _lock = new(LockRecursionPolicy.SupportsRecursion);

    /// <summary>Logger used to report long-held write locks and lock acquisition failures.</summary>
    private readonly ILogger _logger;

    /// <summary>The managed thread ID currently holding the write lock, or <c>null</c> if none.</summary>
    private int? _writeLockHolderThreadId;

    /// <summary>UTC timestamp at which the current write lock was acquired, if any.</summary>
    private DateTime? _writeLockAcquiredAtUtc;

    /// <summary>
    /// Creates a new platform reader/writer lock.
    /// </summary>
    /// <param name="loggerFactory">Factory used to obtain a logger scoped to this lock.</param>
    public PlatformReaderWriterLock(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.GetLogger(typeof(PlatformReaderWriterLock));
    }

    /// <summary>Indicates whether the calling thread currently holds the read lock (directly or via recursion).</summary>
    public bool IsReadLockHeldByCurrentThread => _lock.IsReadLockHeld;

    /// <summary>Indicates whether the calling thread currently holds the write lock (directly or via recursion).</summary>
    public bool IsWriteLockHeldByCurrentThread => _lock.IsWriteLockHeld;

    /// <summary>The managed thread ID currently holding the write lock, or <c>null</c> if no thread holds it.</summary>
    public int? WriteLockHolderThreadId => _writeLockHolderThreadId;

    /// <summary>
    /// Acquires the shared read lock, blocking the calling thread until it is available.
    /// Must be paired with a call to <see cref="ExitReadLock"/>.
    /// </summary>
    public void EnterReadLock()
    {
        _lock.EnterReadLock();
    }

    /// <summary>
    /// Releases a previously acquired read lock.
    /// </summary>
    public void ExitReadLock()
    {
        _lock.ExitReadLock();
    }

    /// <summary>
    /// Attempts to acquire the shared read lock within the given timeout.
    /// </summary>
    /// <param name="timeout">The maximum time to wait for the lock to become available.</param>
    /// <returns><c>true</c> if the lock was acquired; otherwise <c>false</c>.</returns>
    public bool TryEnterReadLock(TimeSpan timeout)
    {
        return _lock.TryEnterReadLock(timeout);
    }

    /// <summary>
    /// Acquires the exclusive write lock, blocking the calling thread until all
    /// current readers and any other writer have released it. Records the holding
    /// thread and acquisition time for diagnostics.
    /// Must be paired with a call to <see cref="ExitWriteLock"/>.
    /// </summary>
    public void EnterWriteLock()
    {
        _lock.EnterWriteLock();
        _writeLockHolderThreadId = Environment.CurrentManagedThreadId;
        _writeLockAcquiredAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Releases a previously acquired write lock and clears diagnostic tracking.
    /// If the lock was held for longer than a suspicious threshold, logs a warning
    /// to help identify slow write actions.
    /// </summary>
    public void ExitWriteLock()
    {
        if (_writeLockAcquiredAtUtc is { } acquiredAt)
        {
            var heldFor = DateTime.UtcNow - acquiredAt;

            if (heldFor > TimeSpan.FromSeconds(2))
            {
                _logger.Warn(
                    "Write lock was held for {0}ms by thread {1}, which is unusually long",
                    (int)heldFor.TotalMilliseconds,
                    _writeLockHolderThreadId ?? -1);
            }
        }

        _writeLockHolderThreadId = null;
        _writeLockAcquiredAtUtc = null;
        _lock.ExitWriteLock();
    }

    /// <summary>
    /// Releases the underlying lock's resources. Must not be called while any
    /// read or write lock is currently held.
    /// </summary>
    public void Dispose()
    {
        _lock.Dispose();
    }
}