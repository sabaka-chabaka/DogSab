using DogSab.Platform.Vfs.Abstractions.VirtualFile;

namespace DogSab.Platform.Indexing.Abstractions.Index;

/// <summary>
/// Declares how to extract indexable (key, value) pairs from a single file's
/// content for a specific index. Implemented by platform subsystems or
/// plugins that want to make some derived fact about files quickly
/// searchable — e.g. a class-name index extension reads a C# file and yields
/// (className, filePath) pairs for every class declaration found. The
/// indexing engine calls this once per file whenever that file is built or
/// rebuilt, never re-deriving the same fact by re-parsing the file at query time.
/// </summary>
/// <typeparam name="TKey">The type of key files are indexed by (e.g. a class name string).</typeparam>
/// <typeparam name="TValue">The type of value associated with each key (e.g. a file path or richer location record).</typeparam>
public interface IIndexExtension<TKey, TValue>
    where TKey : notnull
{
    /// <summary>The identifier of the index this extension contributes to.</summary>
    IndexId IndexId { get; }

    /// <summary>
    /// Determines whether this extension applies to a given file at all,
    /// checked before <see cref="Index"/> is called, so files that obviously
    /// don't apply (e.g. a binary file for a C#-specific index) are skipped
    /// without opening them.
    /// </summary>
    /// <param name="file">The file to check.</param>
    /// <returns><c>true</c> if this extension should index <paramref name="file"/>; otherwise <c>false</c>.</returns>
    bool AppliesTo(IVirtualFile file);

    /// <summary>
    /// Extracts the (key, value) pairs this file contributes to the index.
    /// Called once whenever the file is built or rebuilt.
    /// </summary>
    /// <param name="file">The file to index.</param>
    /// <returns>The key/value pairs this file contributes.</returns>
    IEnumerable<KeyValuePair<TKey, TValue>> Index(IVirtualFile file);
}