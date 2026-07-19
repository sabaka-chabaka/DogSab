namespace DogSab.Platform.Core.Threading.Impl.ReadWrite;

/// <summary>
/// Disposable scope that enters the exclusive write lock on construction and
/// releases it on disposal. Used internally by <see cref="ReadWriteActionManagerImpl"/>
/// to guarantee the lock is released even if the wrapped action throws.
/// </summary>
internal readonly struct WriteActionScope : IDisposable
{
    /// <summary>The lock this scope acquired the write lock on.</summary>
    private readonly PlatformReaderWriterLock _platformLock;

    /// <summary>
    /// Enters the exclusive write lock on <paramref name="platformLock"/>.
    /// </summary>
    /// <param name="platformLock">The lock to acquire the write lock on for the lifetime of this scope.</param>
    public WriteActionScope(PlatformReaderWriterLock platformLock)
    {
        _platformLock = platformLock;
        _platformLock.EnterWriteLock();
    }

    /// <summary>
    /// Releases the write lock acquired by this scope.
    /// </summary>
    public void Dispose()
    {
        _platformLock.ExitWriteLock();
    }
}