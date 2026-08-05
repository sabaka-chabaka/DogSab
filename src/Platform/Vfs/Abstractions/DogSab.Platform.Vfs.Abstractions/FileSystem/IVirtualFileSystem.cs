using DogSab.Platform.Vfs.Abstractions.VirtualFile;

namespace DogSab.Platform.Vfs.Abstractions.FileSystem;

/// <summary>
/// A provider capable of resolving, reading, and (where supported) writing
/// files identified by <see cref="IVirtualFile"/>. Write and structural
/// operations live here rather than on <see cref="IVirtualFile"/> itself,
/// since a virtual file is just a stable identity/address, while the file
/// system is what actually knows how to perform I/O for its particular
/// backing store — and some backing stores (e.g. <see cref="VirtualFileSystemScheme.Archive"/>)
/// may not support writing at all.
/// </summary>
public interface IVirtualFileSystem
{
    /// <summary>The scheme this file system handles, matching a constant from <see cref="VirtualFileSystemScheme"/>.</summary>
    string Scheme { get; }

    /// <summary>Whether this file system supports write operations. If <c>false</c>, write methods always throw.</summary>
    bool IsWritable { get; }

    /// <summary>
    /// Resolves a full virtual path (including scheme) to a virtual file, if it exists.
    /// </summary>
    /// <param name="fullPath">The full virtual path to resolve.</param>
    /// <returns>The resolved file, or <c>null</c> if nothing exists at that path.</returns>
    IVirtualFile? FindFile(string fullPath);

    /// <summary>
    /// Writes new content to a file, replacing its existing content entirely.
    /// </summary>
    /// <param name="file">The file to write to.</param>
    /// <param name="content">The new content.</param>
    /// <param name="cancellationToken">Token used to cancel the write.</param>
    /// <returns>A task that completes when the write has finished.</returns>
    /// <exception cref="System.NotSupportedException">Thrown if <see cref="IsWritable"/> is <c>false</c>.</exception>
    Task WriteAsync(IVirtualFile file, Stream content, CancellationToken cancellationToken);

    /// <summary>
    /// Creates a new file at the given path with the given initial content.
    /// </summary>
    /// <param name="directory">The parent directory to create the file in.</param>
    /// <param name="fileName">The new file's name.</param>
    /// <param name="content">The file's initial content.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The newly created virtual file.</returns>
    /// <exception cref="System.NotSupportedException">Thrown if <see cref="IsWritable"/> is <c>false</c>.</exception>
    Task<IVirtualFile> CreateFileAsync(IVirtualFile directory, string fileName, Stream content, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a file or directory (recursively, if a directory).
    /// </summary>
    /// <param name="file">The file or directory to delete.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task that completes when the deletion has finished.</returns>
    /// <exception cref="System.NotSupportedException">Thrown if <see cref="IsWritable"/> is <c>false</c>.</exception>
    Task DeleteAsync(IVirtualFile file, CancellationToken cancellationToken);
}