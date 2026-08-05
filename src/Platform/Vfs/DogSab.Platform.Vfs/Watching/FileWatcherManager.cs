using System.Collections.Concurrent;
using DogSab.Platform.Core.Abstractions.Logging;
using DogSab.Platform.Core.Abstractions.Messaging;
using DogSab.Platform.Vfs.Abstractions.FileSystem;
using DogSab.Platform.Vfs.Abstractions.VirtualFile;
using DogSab.Platform.Vfs.Abstractions.Watching;
using DogSab.Platform.Vfs.FileSystem;
using DogSab.Platform.Vfs.VirtualFile;

namespace DogSab.Platform.Vfs.Watching;

/// <summary>
/// Ties together <see cref="NativeFileWatcher"/> (raw OS notifications),
/// <see cref="FileChangeDebouncer"/> (collapsing bursts into one event per
/// path), and the platform's message bus: for each watched directory, raw
/// native events are debounced, then resolved back into an
/// <see cref="IVirtualFile"/> and published to both
/// <see cref="VfsTopics.FILE_CHANGED_UI"/> and
/// <see cref="VfsTopics.FILE_CHANGED_BACKGROUND"/>, so every kind of
/// subscriber (UI-thread or background) observes the same settled change exactly once.
/// </summary>
public sealed class FileWatcherManager : IDisposable
{
    /// <summary>Default window used to debounce bursts of native file system events.</summary>
    private static readonly TimeSpan DefaultDebounceWindow = TimeSpan.FromMilliseconds(100);

    private readonly VirtualFileSystemRouter _router;
    private readonly IMessageBus _messageBus;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger _logger;

    /// <summary>Active native watchers, keyed by the directory they watch.</summary>
    private readonly ConcurrentDictionary<string, NativeFileWatcher> _watchersByDirectory = new();

    /// <summary>Active debouncers, keyed by the directory they debounce events for.</summary>
    private readonly ConcurrentDictionary<string, FileChangeDebouncer> _debouncersByDirectory = new();

    /// <summary>
    /// Creates a new file watcher manager.
    /// </summary>
    public FileWatcherManager(VirtualFileSystemRouter router, IMessageBus messageBus, ILoggerFactory loggerFactory)
    {
        _router = router;
        _messageBus = messageBus;
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.GetLogger(typeof(FileWatcherManager));
    }

    /// <summary>
    /// Starts watching a directory (recursively) for changes, publishing
    /// settled, debounced changes to both VFS topics.
    /// </summary>
    /// <param name="directoryDiskPath">The absolute disk path of the directory to watch.</param>
    public void StartWatching(string directoryDiskPath)
    {
        if (_watchersByDirectory.ContainsKey(directoryDiskPath))
        {
            return;
        }

        var debouncer = new FileChangeDebouncer(DefaultDebounceWindow, path => OnSettledChange(directoryDiskPath, path));
        _debouncersByDirectory[directoryDiskPath] = debouncer;

        var nativeWatcher = new NativeFileWatcher(directoryDiskPath, _loggerFactory);
        nativeWatcher.RawChangeDetected += debouncer.Notify;

        _watchersByDirectory[directoryDiskPath] = nativeWatcher;

        _logger.Info("Started watching directory '{0}'.", directoryDiskPath);
    }

    /// <summary>
    /// Stops watching a previously watched directory.
    /// </summary>
    /// <param name="directoryDiskPath">The directory to stop watching.</param>
    public void StopWatching(string directoryDiskPath)
    {
        if (_watchersByDirectory.TryRemove(directoryDiskPath, out var watcher))
        {
            watcher.Dispose();
        }

        if (_debouncersByDirectory.TryRemove(directoryDiskPath, out var debouncer))
        {
            debouncer.Dispose();
        }
    }

    /// <summary>
    /// Called once per settled (debounced) path change. Resolves the affected
    /// path back into an <see cref="IVirtualFile"/> and publishes the change
    /// on both VFS topics.
    /// </summary>
    /// <param name="watchedDirectory">The watched directory this change occurred under, for logging.</param>
    /// <param name="diskPath">The affected file's disk path.</param>
    private void OnSettledChange(string watchedDirectory, string diskPath)
    {
        var virtualPath = VirtualFilePathParser.Combine(VirtualFileSystemScheme.Local, diskPath.Replace('\\', '/'));
        var file = _router.FindFile(virtualPath);

        var eventArgs = file is not null
            ? new FileChangeEvent(virtualPath, FileChangeType.Changed, DateTime.UtcNow, file)
            : new FileChangeEvent(virtualPath, FileChangeType.Deleted, DateTime.UtcNow);

        _messageBus.Publisher(VfsTopics.FILE_CHANGED_UI).OnFileChanged(eventArgs);
        _messageBus.Publisher(VfsTopics.FILE_CHANGED_BACKGROUND).OnFileChanged(eventArgs);
    }

    /// <summary>
    /// Stops all active watchers and debouncers.
    /// </summary>
    public void Dispose()
    {
        foreach (var watcher in _watchersByDirectory.Values)
        {
            watcher.Dispose();
        }

        foreach (var debouncer in _debouncersByDirectory.Values)
        {
            debouncer.Dispose();
        }

        _watchersByDirectory.Clear();
        _debouncersByDirectory.Clear();
    }
}