using System.Collections.Concurrent;
using DogSab.Platform.Indexing.Abstractions.Index;

namespace DogSab.Platform.Indexing.Storage;

/// <summary>
/// First, simplest implementation of <see cref="IIndexStorage"/>: everything
/// lives in an in-process dictionary, nothing is persisted to disk. Rebuilt
/// from scratch on every application restart. A future
/// <c>SqliteIndexStorage</c> can implement the same interface for persistent,
/// cross-session indexes without any change to <see cref="Index.IndexImpl{TKey,TValue}"/>
/// or callers above it.
/// </summary>
internal sealed class InMemoryIndexStorage : IIndexStorage
{
    /// <summary>Entries keyed by the boxed key, each holding the list of (value, sourceFilePath) pairs contributed for it.</summary>
    private readonly ConcurrentDictionary<object, List<(object Value, string SourceFilePath)>> _entriesByKey = new();

    /// <summary>Guards mutation of an individual key's entry list, since List&lt;T&gt; itself is not thread-safe.</summary>
    private readonly ConcurrentDictionary<object, object> _keyLocks = new();

    /// <inheritdoc />
    public IndexId IndexId { get; }

    /// <summary>
    /// Creates a new in-memory storage instance for a specific index.
    /// </summary>
    /// <param name="indexId">The identifier of the index this storage backs.</param>
    public InMemoryIndexStorage(IndexId indexId)
    {
        IndexId = indexId;
    }

    /// <inheritdoc />
    public void RemoveEntriesForFile(string filePath)
    {
        foreach (var key in _entriesByKey.Keys.ToArray())
        {
            var lockObj = _keyLocks.GetOrAdd(key, static _ => new object());

            lock (lockObj)
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
        var lockObj = _keyLocks.GetOrAdd(key, static _ => new object());

        lock (lockObj)
        {
            entries.Add((value, sourceFilePath));
        }
    }

    /// <inheritdoc />
    public IReadOnlyList<object> GetValues(object key)
    {
        if (!_entriesByKey.TryGetValue(key, out var entries))
        {
            return System.Array.Empty<object>();
        }

        var lockObj = _keyLocks.GetOrAdd(key, static _ => new object());

        lock (lockObj)
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