using System.Collections.Concurrent;

namespace DogSab.Platform.Vfs.Watching;

/// <summary>
/// Collapses bursts of raw change notifications for the same path into a
/// single callback, delayed by a short window. <see cref="System.IO.FileSystemWatcher"/>
/// is notorious for firing multiple events for what is, from the user's
/// perspective, a single logical change — e.g. some editors write a file by
/// creating a temp file, writing to it, then renaming it over the original,
/// which can surface as Create + Change + Delete + Create in quick
/// succession. Without debouncing, every downstream consumer (Indexing,
/// Project View) would redundantly react multiple times per real edit.
/// </summary>
public sealed class FileChangeDebouncer : IDisposable
{
    /// <summary>How long to wait after the last observed event for a path before firing the debounced callback.</summary>
    private readonly TimeSpan _debounceWindow;

    /// <summary>The callback to invoke once a path's burst of events has settled.</summary>
    private readonly Action<string> _onSettled;

    /// <summary>Pending timers per path, replaced (reset) each time a new event arrives for that path within the window.</summary>
    private readonly ConcurrentDictionary<string, Timer> _pendingTimersByPath = new();

    /// <summary>
    /// Creates a new debouncer.
    /// </summary>
    /// <param name="debounceWindow">How long to wait after the last event for a path before firing.</param>
    /// <param name="onSettled">Callback invoked once per path, after its burst of events has settled.</param>
    public FileChangeDebouncer(TimeSpan debounceWindow, Action<string> onSettled)
    {
        _debounceWindow = debounceWindow;
        _onSettled = onSettled;
    }

    /// <summary>
    /// Records a raw change event for a path, resetting its debounce timer.
    /// If no further events for this path arrive within the debounce window,
    /// the callback given at construction fires exactly once for this path.
    /// </summary>
    /// <param name="path">The disk path the raw event was observed for.</param>
    public void Notify(string path)
    {
        var timer = _pendingTimersByPath.AddOrUpdate(
            path,
            addValueFactory: _ => CreateTimer(path),
            updateValueFactory: (_, existingTimer) =>
            {
                existingTimer.Change(_debounceWindow, Timeout.InfiniteTimeSpan);
                return existingTimer;
            });

        // AddOrUpdate's addValueFactory already creates a fresh, correctly-scheduled
        // timer, so no further action is needed for the "added" branch here.
        _ = timer;
    }

    /// <summary>
    /// Creates a new one-shot timer for a path, scheduled to fire once the
    /// debounce window elapses without further activity.
    /// </summary>
    /// <param name="path">The path this timer is tracking.</param>
    /// <returns>The newly created timer.</returns>
    private Timer CreateTimer(string path)
    {
        return new Timer(
            callback: _ => FireSettled(path),
            state: null,
            dueTime: _debounceWindow,
            period: Timeout.InfiniteTimeSpan);
    }

    /// <summary>
    /// Invoked when a path's debounce window has elapsed without further
    /// events; removes its timer and invokes the settled callback.
    /// </summary>
    /// <param name="path">The path whose burst of events has settled.</param>
    private void FireSettled(string path)
    {
        if (_pendingTimersByPath.TryRemove(path, out var timer))
        {
            timer.Dispose();
        }

        _onSettled(path);
    }

    /// <summary>
    /// Disposes all pending timers without firing their callbacks. Any change
    /// still within its debounce window at disposal time is silently dropped.
    /// </summary>
    public void Dispose()
    {
        foreach (var timer in _pendingTimersByPath.Values)
        {
            timer.Dispose();
        }

        _pendingTimersByPath.Clear();
    }
}