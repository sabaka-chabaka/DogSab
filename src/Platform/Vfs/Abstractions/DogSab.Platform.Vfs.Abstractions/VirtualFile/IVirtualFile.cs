namespace DogSab.Platform.Vfs.Abstractions.VirtualFile;

/// <summary>
/// A stable identity for a file or directory within the platform's virtual
/// file system, independent of which concrete <see cref="FileSystem.IVirtualFileSystem"/>
/// backs it (local disk, in-memory, or inside an archive). Platform
/// subsystems (Editor, Indexing, Psi) work against this identity rather than
/// raw path strings, so the same file requested by different subsystems
/// resolves to the same object, and there is a single point at which to
/// observe changes to it.
/// </summary>
public interface IVirtualFile
{
    /// <summary>The file or directory's simple name (e.g. <c>"Program.cs"</c>), without any path component.</summary>
    string Name { get; }
    
    /// <summary>
    /// The full virtual path, including its scheme prefix (e.g.
    /// <c>"file:///home/user/project/Program.cs"</c>). Uniquely identifies
    /// this file across the platfform, including across differenct backing
    /// file systems, since the scheme disambiguates them.
    /// </summary>
    string Path { get; }
    
    /// <summary>Whether this represents a file or a directory.</summary>
    VirtualFileType Type { get; }

    /// <summary>The file system that owns and can resolve operations on this file.</summary>
    FileSystem.IVirtualFileSystem FileSystem { get; }

    /// <summary>The parent directory, or <c>null</c> if this is a root (e.g. a mounted drive or archive root).</summary>
    IVirtualFile? Parent { get; }

    /// <summary>Whether this file is still valid — <c>false</c> after the underlying file has been deleted or the file system unmounted.</summary>
    bool IsValid { get; }
    
    /// <summary>
    /// Lists the immediate children of this entry. Only meaningful when
    /// <see cref="Type"/> is <see cref="VirtualFileType.Directory"/>.
    /// </summary>
    /// <returns>The child files and directories, or an empty list if this is a file, not a directory.</returns>
    IReadOnlyList<IVirtualFile> GetChildren();

    /// <summary>
    /// Opens a stream to read this file's content. Only meaningful when
    /// <see cref="Type"/> is <see cref="VirtualFileType.File"/>.
    /// </summary>
    /// <returns>A readable stream over the file's current content.</returns>
    /// <exception cref="InvalidOperationException">Thrown if called on a directory.</exception>
    Stream OpenRead();

    /// <summary>The length of the file's content in bytes. Only meaningful for files, not directories.</summary>
    long Length { get; }

    /// <summary>The UTC timestamp this file's content was last modified.</summary>
    DateTime LastModifiedUtc { get; }
}