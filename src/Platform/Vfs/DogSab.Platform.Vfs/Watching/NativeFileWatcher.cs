using DogSab.Platform.Core.Abstractions.Logging;

namespace DogSab.Platform.Vfs.Watching;

/// <summary>
/// Thin wrapper around <see cref="FileSystemWatcher"/> for a single watched
/// directory (recursive). Normalizes its raw Created/Changed/Deleted/Renamed
/// events into a single <see cref="RawChangeDetected"/> callback carrying just
/// the affected path, so callers (specifically <see cref="FileWatcherManager"/>)
/// don't need to handle each native event type separately before debouncing.
/// </summary>
public sealed class NativeFileWatcher : IDisposable
{
    /// <summary>The underlying .NET file system watcher.</summary>
    private readonly FileSystemWatcher _watcher;

    /// <summary>Logger used to report watcher errors (e.g. internal buffer overflow).</summary>
    private readonly ILogger _logger;

    /// <summary>Raised whenever a raw change is detected for a path under the watched directory.</summary>
    public event Action<string>? RawChangeDetected;

    /// <summary>
    /// Creates a new watcher for a directory and starts watching immediately.
    /// </summary>
    /// <param name="watchedDirectory">The absolute path to the directory to watch, recursively.</param>
    /// <param name="loggerFactory">Factory used to obtain a logger scoped to this watcher.</param>
    public NativeFileWatcher(string watchedDirectory, ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.GetLogger(typeof(NativeFileWatcher));

        _watcher = new FileSystemWatcher(watchedDirectory)
        {
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName
        };

        _watcher.Created += (_, e) => RawChangeDetected?.Invoke(e.FullPath);
        _watcher.Changed += (_, e) => RawChangeDetected?.Invoke(e.FullPath);
        _watcher.Deleted += (_, e) => RawChangeDetected?.Invoke(e.FullPath);
        _watcher.Renamed += (_, e) =>
        {
            RawChangeDetected?.Invoke(e.OldFullPath);
            RawChangeDetected?.Invoke(e.FullPath);
        };
        _watcher.Error += (_, e) => _logger.Error(
            "FileSystemWatcher error for directory '{0}' — watcher may need to be recreated",
            e.GetException(),
            watchedDirectory);

        _watcher.EnableRaisingEvents = true;
    }

    /// <summary>
    /// Stops watching and releases the underlying <see cref="FileSystemWatcher"/>.
    /// </summary>
    public void Dispose()
    {
        _watcher.EnableRaisingEvents = false;
        _watcher.Dispose();
    }
}