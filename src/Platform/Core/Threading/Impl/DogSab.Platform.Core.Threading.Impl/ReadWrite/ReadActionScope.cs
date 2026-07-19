namespace DogSab.Platform.Core.Threading.Impl.ReadWrite;

/// <summary>
/// Disposable scope that enters the shared read lock on construction and
/// releases it on disposal. Used internally by <see cref="ReadWriteActionManagerImpl"/>
/// to guarantee the lock is released even if the wrapped action throws.
/// </summary>
public readonly struct ReadActionScope : IDisposable
{
    /// <summary>The lock this scope acquired a read lock on.</summary>
    private readonly PlatformReaderWriterLock _platformLock;

    /// <summary>
    /// Enters the shared read lock on <paramref name="platformLock"/>.
    /// </summary>
    /// <param name="platformLock">The lock to acquire a read lock on for the lifetime of this scope.</param>
    public ReadActionScope(PlatformReaderWriterLock platformLock)
    {
        _platformLock = platformLock;
        _platformLock.EnterReadLock();
    }

    /// <summary>
    /// Releases the read lock acquired by this scope.
    /// </summary>
    public void Dispose()
    {
        _platformLock.ExitReadLock();
    }
}