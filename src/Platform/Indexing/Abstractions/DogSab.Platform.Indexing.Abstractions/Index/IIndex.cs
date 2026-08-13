namespace DogSab.Platform.Indexing.Abstractions.Index;

/// <summary>
/// A queryable, already-built index: given a key, returns every value that
/// was contributed for it across all indexed files. Consumers (e.g. "Find
/// Usages", "Go to Class") query this rather than re-scanning files
/// themselves — all the expensive extraction work already happened during
/// indexing, via the matching <see cref="IIndexExtension{TKey,TValue}"/>.
/// </summary>
/// <typeparam name="TKey">The type of key this index is queried by.</typeparam>
/// <typeparam name="TValue">The type of value stored per key.</typeparam>
public interface IIndex<TKey, TValue>
    where TKey : notnull
{
    /// <summary>The identifier of this index.</summary>
    IndexId IndexId { get; }

    /// <summary>
    /// Returns every value currently associated with a key, across all
    /// indexed files. Returns an empty collection if the key has no entries.
    /// </summary>
    /// <param name="key">The key to look up.</param>
    /// <returns>The values associated with <paramref name="key"/>.</returns>
    IReadOnlyList<TValue> Get(TKey key);

    /// <summary>
    /// Returns every distinct key currently present in the index. Useful for
    /// features like autocomplete over all known class names.
    /// </summary>
    /// <returns>All keys currently present in the index.</returns>
    IReadOnlyList<TKey> GetAllKeys();
}