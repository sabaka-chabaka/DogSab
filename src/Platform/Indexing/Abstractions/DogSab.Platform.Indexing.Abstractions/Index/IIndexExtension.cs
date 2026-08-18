using DogSab.Platform.Vfs.Abstractions.VirtualFile;

namespace DogSab.Platform.Indexing.Abstractions.Index;

/// <summary>
/// Non-generic base contract for an index extension, used by the indexing
/// engine (<c>IndexBuildWorker</c>) to invoke extensions without needing to
/// know their specific <c>TKey</c>/<c>TValue</c> at compile time — mirroring
/// how <c>IExtensionPointRegistry</c>'s untyped registration path lets the
/// plugin loader register extensions whose contract type is only known at
/// runtime. Extension authors should implement
/// <see cref="IIndexExtension{TKey,TValue}"/> instead of this directly; that
/// generic interface's default <see cref="IndexUntyped"/> implementation
/// bridges to <see cref="IIndexExtension{TKey,TValue}.Index"/> automatically.
/// </summary>
public interface IIndexExtension
{
    /// <summary>The identifier of the index this extension contributes to.</summary>
    IndexId IndexId { get; }

    /// <summary>
    /// Determines whether this extension applies to a given file at all.
    /// </summary>
    /// <param name="file">The file to check.</param>
    /// <returns><c>true</c> if this extension should index <paramref name="file"/>; otherwise <c>false</c>.</returns>
    bool AppliesTo(IVirtualFile file);

    /// <summary>
    /// Extracts this file's contributed entries as boxed (key, value) pairs,
    /// without requiring the caller to know the extension's actual
    /// <c>TKey</c>/<c>TValue</c> types.
    /// </summary>
    /// <param name="file">The file to index.</param>
    /// <returns>The key/value pairs this file contributes, boxed as <see cref="object"/>.</returns>
    IEnumerable<KeyValuePair<object, object>> IndexUntyped(IVirtualFile file);
}

/// <summary>
/// Typed contract for an index extension, implemented by extension authors.
/// Declares how to extract indexable (key, value) pairs from a single file's
/// content for a specific index. The indexing engine calls this once per
/// file whenever that file is built or rebuilt, never re-deriving the same
/// fact by re-parsing the file at query time.
/// </summary>
/// <typeparam name="TKey">The type of key files are indexed by (e.g. a class name string).</typeparam>
/// <typeparam name="TValue">The type of value associated with each key (e.g. a file path or richer location record).</typeparam>
public interface IIndexExtension<TKey, TValue> : IIndexExtension
    where TKey : notnull
{
    /// <summary>
    /// Extracts the (key, value) pairs this file contributes to the index.
    /// Called once whenever the file is built or rebuilt.
    /// </summary>
    /// <param name="file">The file to index.</param>
    /// <returns>The key/value pairs this file contributes.</returns>
    IEnumerable<KeyValuePair<TKey, TValue>> Index(IVirtualFile file);

    /// <summary>
    /// Default implementation of the untyped bridge: calls <see cref="Index"/>
    /// and boxes each key and value as <see cref="object"/>. Extension
    /// authors do not need to override this.
    /// </summary>
    IEnumerable<KeyValuePair<object, object>> IIndexExtension.IndexUntyped(IVirtualFile file)
    {
        foreach (var pair in Index(file))
        {
            yield return new KeyValuePair<object, object>(pair.Key, pair.Value);
        }
    }
}