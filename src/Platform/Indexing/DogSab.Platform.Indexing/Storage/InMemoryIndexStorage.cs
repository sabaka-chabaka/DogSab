using System.Collections.Concurrent;
using DogSab.Platform.Indexing.Abstractions.Index;

namespace DogSab.Platform.Indexing.Storage;

/// <summary>
/// First, simplest implementation of <see cref="IIndexStorage"/>: everything
/// lives in an in-process dictionary, nothing is persisted to disk. Maintains
/// a reverse index (file path → contributed keys) alongside the forward index
/// (key → values), so <see cref="RemoveEntriesForFile"/> can find and remove
/// a file's stale entries in time proportional to that file's own entry
/// count, not the total number of keys across the whole index — important
/// since this runs on every reindex of a single saved file.
/// </summary>
internal sealed class InMemoryIndexStorage : IIndexStorage
{
    /// <summary>Entries keyed by the boxed key, each holding the list of (value, sourceFilePath) pairs contributed for it.</summary>
    private readonly ConcurrentDictionary<object, List<(object Value, string SourceFilePath)>> _entriesByKey = new();

    /// <summary>Reverse index: for each file path, the set of keys it has contributed entries under, enabling fast removal.</summary>
    private readonly ConcurrentDictionary<string, HashSet<object>> _keysByFilePath = new();

    /// <summary>Guards mutation of an individual key's entry list.</summary>
    private readonly ConcurrentDictionary<object, object> _keyLocks = new();

    /// <summary>Guards mutation of an individual file's key set in the reverse index.</summary>
    private readonly ConcurrentDictionary<string, object> _fileLocks = new();

    /// <inheritdoc />
    public IndexId IndexId { get; }

    public InMemoryIndexStorage(IndexId indexId)
    {
        IndexId = indexId;
    }

    /// <inheritdoc />
    public void RemoveEntriesForFile(string filePath)
    {
        var fileLockObj = _fileLocks.GetOrAdd(filePath, static _ => new object());

        HashSet<object>? keysForFile;

        lock (fileLockObj)
        {
            if (!_keysByFilePath.TryRemove(filePath, out keysForFile))
            {
                return;
            }
        }

        foreach (var key in keysForFile)
        {
            var keyLockObj = _keyLocks.GetOrAdd(key, static _ => new object());

            lock (keyLockObj)
            {
                if (_entriesByKey.TryGetValue(key, out var entries))
                {
                    entries.RemoveAll(e => e.SourceFilePath == filePath);

                    if (entries.Count == 0)
                    {
                        _entriesByKey.TryRemove(key, out _);
                    }
                }
            }
        }
    }

    /// <inheritdoc />
    public void AddEntry(object key, object value, string sourceFilePath)
    {
        var entries = _entriesByKey.GetOrAdd(key, static _ => new List<(object, string)>());
        var keyLockObj = _keyLocks.GetOrAdd(key, static _ => new object());

        lock (keyLockObj)
        {
            entries.Add((value, sourceFilePath));
        }

        var keysForFile = _keysByFilePath.GetOrAdd(sourceFilePath, static _ => new HashSet<object>());
        var fileLockObj = _fileLocks.GetOrAdd(sourceFilePath, static _ => new object());

        lock (fileLockObj)
        {
            keysForFile.Add(key);
        }
    }

    /// <inheritdoc />
    public IReadOnlyList<object> GetValues(object key)
    {
        if (!_entriesByKey.TryGetValue(key, out var entries))
        {
            return System.Array.Empty<object>();
        }

        var keyLockObj = _keyLocks.GetOrAdd(key, static _ => new object());

        lock (keyLockObj)
        {
            return entries.Select(e => e.Value).ToList();
        }
    }

    /// <inheritdoc />
    public IReadOnlyList<object> GetAllKeys()
    {
        return _entriesByKey.Keys.ToList();
    }
}