using System.Collections.Concurrent;
using DogSab.Platform.Indexing.Abstractions.Index;
using DogSab.Platform.Indexing.Storage;
using Microsoft.Win32;

namespace DogSab.Platform.Indexing.Index;

/// <summary>
/// Holds every declared index's storage and extension, keyed by
/// <see cref="IndexId"/>. Registration is untyped internally (storage and
/// extensions are boxed as <see cref="object"/>) for the same reason as
/// <see cref="Registry.ExtensionPointRegistryImpl"/> —
/// different indexes have different <c>TKey</c>/<c>TValue</c> pairs and
/// cannot share a single generically-typed collection. Typed access is
/// provided through <see cref="GetIndex{TKey,TValue}"/>, which performs a
/// runtime check and reports a clear error on mismatch rather than a bare cast failure.
/// </summary>
public sealed class IndexRegistry
{
    private readonly ConcurrentDictionary<IndexId, IIndexStorage> _storageById = new();
    private readonly ConcurrentDictionary<IndexId, object> _extensionsById = new();

    /// <summary>
    /// Registers an index extension, creating its backing storage.
    /// </summary>
    /// <typeparam name="TKey">The index's key type.</typeparam>
    /// <typeparam name="TValue">The index's value type.</typeparam>
    /// <param name="extension">The extension declaring how to derive entries for this index.</param>
    /// <exception cref="InvalidOperationException">Thrown if an index is already registered under the same <see cref="IndexId"/>.</exception>
    public void RegisterIndex<TKey, TValue>(IIndexExtension<TKey, TValue> extension) where TKey : notnull
    {
        var storage = new InMemoryIndexStorage(extension.IndexId);

        if (!_storageById.TryAdd(extension.IndexId, storage))
        {
            throw new InvalidOperationException($"An index is already registered under id '{extension.IndexId}'.");
        }

        _extensionsById[extension.IndexId] = extension;
    }
    
    /// <summary>
    /// Resolves the extension registered for an index, typed.
    /// </summary>
    /// <typeparam name="TKey">The expected key type.</typeparam>
    /// <typeparam name="TValue">The expected value type.</typeparam>
    /// <param name="indexId">The index to look up.</param>
    /// <returns>The registered extension.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no extension is registered, or its types don't match.</exception>
    public IIndexExtension<TKey, TValue> GetExtension<TKey, TValue>(IndexId indexId) where TKey : notnull
    {
        if (!_extensionsById.TryGetValue(indexId, out var raw))
        {
            throw new InvalidOperationException($"No index extension registered under id '{indexId}'.");
        }
        
        return raw as IIndexExtension<TKey, TValue> ?? 
               throw new InvalidOperationException($"Index '{indexId}' was registered with a different (TKey, TValue) pair than requested " +
                $"({typeof(TKey).Name}, {typeof(TValue).Name}); actual registered type is '{raw.GetType().FullName}'.");
    }

    /// <summary>
    /// Returns a typed, queryable view over an index's storage.
    /// </summary>
    /// <typeparam name="TKey">The expected key type.</typeparam>
    /// <typeparam name="TValue">The expected value type.</typeparam>
    /// <param name="indexId">The index to query.</param>
    /// <returns>A typed <see cref="IIndex{TKey,TValue}"/> view over the index's data.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no storage is registered under this ID.</exception>
    public IIndex<TKey, TValue> GetIndex<TKey, TValue>(IndexId indexId) where TKey : notnull
    {
        if (!_storageById.TryGetValue(indexId, out var storage))
        {
            throw new InvalidOperationException($"No index registered under id '{indexId}'.");
        }

        return new IndexImpl<TKey, TValue>(storage);
    }
    
    /// <summary>
    /// Returns the untyped storage for an index, used internally by
    /// <see cref="Building.IndexBuildWorker"/> when writing entries during indexing.
    /// </summary>
    /// <param name="indexId">The index to look up.</param>
    /// <returns>The index's storage.</returns>
    internal IIndexStorage GetStorage(IndexId indexId)
    {
        return _storageById.TryGetValue(indexId, out var storage)
            ? storage
            : throw new InvalidOperationException($"No index registered under id '{indexId}'.");
    }
    
    /// <summary>All declared index IDs, for diagnostics.</summary>
    internal IReadOnlyCollection<IndexId> AllIndexIds => (System.Collections.Generic.IReadOnlyCollection<IndexId>)_storageById.Keys;
}