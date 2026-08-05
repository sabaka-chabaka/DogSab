using DogSab.Platform.Vfs.Abstractions.FileSystem;
using DogSab.Platform.Vfs.Abstractions.VirtualFile;

namespace DogSab.Platform.Vfs.VirtualFile;

/// <summary>
/// An <see cref="IVirtualFile"/> backed by a real path on the local disk.
/// Unlike <see cref="InMemoryVirtualFile"/>, this holds no cached state of
/// its own — every property and method reads live from the underlying disk
/// path on each call, so it always reflects the file system's current state
/// rather than a snapshot taken when the object was created. This means a
/// <see cref="LocalVirtualFile"/> instance remains cheap to hold onto even
/// as the real file changes underneath it.
/// </summary>
public sealed class LocalVirtualFile : IVirtualFile
{
    /// <summary>The real, absolute disk path this entry represents.</summary>
    private readonly string _diskPath;

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public string Path { get; }

    /// <inheritdoc />
    public IVirtualFileSystem FileSystem { get; }

    /// <inheritdoc />
    public IVirtualFile? Parent { get; }

    /// <summary>
    /// Creates a new local virtual file wrapping a real disk path.
    /// </summary>
    /// <param name="diskPath">The absolute path on disk this entry represents.</param>
    /// <param name="virtualPath">The full virtual path (with <c>"file://"</c> scheme) identifying this entry.</param>
    /// <param name="fileSystem">The owning file system.</param>
    /// <param name="parent">The parent directory entry, or <c>null</c> for a root (e.g. a drive).</param>
    public LocalVirtualFile(string diskPath, string virtualPath, IVirtualFileSystem fileSystem, IVirtualFile? parent)
    {
        _diskPath = diskPath;
        Path = virtualPath;
        Name = System.IO.Path.GetFileName(diskPath.TrimEnd(System.IO.Path.DirectorySeparatorChar));
        FileSystem = fileSystem;
        Parent = parent;
    }

    /// <inheritdoc />
    public VirtualFileType Type => Directory.Exists(_diskPath) ? VirtualFileType.Directory : VirtualFileType.File;

    /// <inheritdoc />
    public bool IsValid => File.Exists(_diskPath) || Directory.Exists(_diskPath);

    /// <inheritdoc />
    public long Length => Type == VirtualFileType.File ? new FileInfo(_diskPath).Length : 0;

    /// <inheritdoc />
    public DateTime LastModifiedUtc => File.GetLastWriteTimeUtc(_diskPath);

    /// <summary>The real, absolute disk path this entry represents. Used internally by <see cref="LocalFileSystem"/> for I/O operations.</summary>
    internal string DiskPath => _diskPath;

    /// <inheritdoc />
    public IReadOnlyList<IVirtualFile> GetChildren()
    {
        if (Type != VirtualFileType.Directory)
        {
            return Array.Empty<IVirtualFile>();
        }

        var children = new List<IVirtualFile>();

        foreach (var childDiskPath in Directory.EnumerateFileSystemEntries(_diskPath))
        {
            var childName = System.IO.Path.GetFileName(childDiskPath);
            var childVirtualPath = VirtualFilePathParser.Combine(FileSystem.Scheme, CombineVirtualPath(Path, childName));
            children.Add(new LocalVirtualFile(childDiskPath, childVirtualPath, FileSystem, this));
        }

        return children;
    }

    /// <inheritdoc />
    public Stream OpenRead()
    {
        if (Type != VirtualFileType.File)
        {
            throw new InvalidOperationException($"Cannot open a directory ('{Path}') for reading as a file.");
        }

        return new FileStream(_diskPath, FileMode.Open, FileAccess.Read, FileShare.Read);
    }

    /// <summary>
    /// Appends a child segment to a full virtual path's path portion.
    /// </summary>
    /// <param name="parentVirtualPath">The parent's full virtual path.</param>
    /// <param name="childName">The child's simple name.</param>
    /// <returns>The child's path portion, without scheme.</returns>
    private static string CombineVirtualPath(string parentVirtualPath, string childName)
    {
        var parsed = VirtualFilePathParser.Parse(parentVirtualPath);
        return parsed.Path.TrimEnd('/') + "/" + childName;
    }
}