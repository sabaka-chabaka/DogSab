using DogSab.Platform.Indexing.Abstractions.Index;
using DogSab.Platform.Indexing.Storage;

namespace DogSab.Platform.Indexing.Index;

/// <summary>
/// Typed view over an index's untyped <see cref="IIndexStorage"/>, casting
/// keys and values to <typeparamref name="TKey"/>/<typeparamref name="TValue"/>
/// on read.
/// </summary>
internal sealed class IndexImpl<TKey, TValue> : IIndex<TKey, TValue> where TKey : notnull
{
    private readonly IIndexStorage _storage;

    /// <inheritdoc />
    public IndexId IndexId => _storage.IndexId;

    public IndexImpl(IIndexStorage storage)
    {
        _storage = storage;
    }

    /// <inheritdoc />
    public IReadOnlyList<TValue> Get(TKey key)
    {
        return _storage.GetValues(key).Cast<TValue>().ToList();
    }

    /// <inheritdoc />
    public IReadOnlyList<TKey> GetAllKeys()
    {
        return _storage.GetAllKeys().Cast<TKey>().ToList();
    }
}