using DogSab.Platform.Indexing.Abstractions.Index;

namespace DogSab.Platform.Indexing.Storage;

/// <summary>
/// Internal, untyped storage backend for a single index's data. Stores keys
/// and values as <see cref="object"/> internally so a single storage instance
/// can be created generically at registration time (see
/// <see cref="Index.IndexRegistry"/>) without the storage layer itself needing
/// a compile-time <c>TKey</c>/<c>TValue</c> — those are only known by the
/// caller at query time, via <see cref="Index.IndexImpl{TKey,TValue}"/>, which
/// performs the typed casts and reports a clear error on mismatch rather than
/// letting a bare <see cref="System.InvalidCastException"/> surface.
/// </summary>
internal interface IIndexStorage
{
    /// <summary>The identifier of the index this storage backs.</summary>
    IndexId IndexId { get; }

    /// <summary>
    /// Removes all entries previously contributed by a specific file, in
    /// preparation for re-adding its current entries — called at the start of
    /// (re)indexing a file, so stale entries from a since-changed file don't linger.
    /// </summary>
    /// <param name="filePath">The virtual path of the file whose prior entries should be removed.</param>
    void RemoveEntriesForFile(string filePath);
    
    /// <summary>
    /// Adds a single (key, value) entry, attributed to the file it came from
    /// so it can later be removed via <see cref="RemoveEntriesForFile"/>.
    /// </summary>
    /// <param name="key">The entry's key, boxed as <see cref="object"/>.</param>
    /// <param name="value">The entry's value, boxed as <see cref="object"/>.</param>
    /// <param name="sourceFilePath">The virtual path of the file this entry was derived from.</param>
    void AddEntry(object key, object value, string sourceFilePath);

    /// <summary>
    /// Returns every value currently associated with a key.
    /// </summary>
    /// <param name="key">The key to look up, boxed as <see cref="object"/>.</param>
    /// <returns>The values associated with <paramref name="key"/>, boxed as <see cref="object"/>.</returns>
    IReadOnlyList<object> GetValues(object key);

    /// <summary>
    /// Returns every distinct key currently present in storage.
    /// </summary>
    /// <returns>All keys currently present, boxed as <see cref="object"/>.</returns>
    IReadOnlyList<object> GetAllKeys();
}