using DogSab.Platform.Vfs.Abstractions.FileSystem;
using DogSab.Platform.Vfs.Abstractions.VirtualFile;

namespace DogSab.Platform.Vfs.VirtualFile;

/// <summary>
/// An <see cref="IVirtualFile"/> backed entirely by in-process memory, never
/// persisted to disk. Used for generated/scratch content — e.g. a PSI-derived
/// synthetic file, or an unsaved "new file" buffer before the user chooses to
/// save it to a real location. Content and child entries live directly on
/// this object rather than being looked up from an external store, since
/// <see cref="InMemoryFileSystem"/> is itself just a registry of these objects.
/// </summary>
public sealed class InMemoryVirtualFile : IVirtualFile
{
    /// <summary>Backing content for a file entry; unused for directories.</summary>
    private byte[] _content;

    /// <summary>Child entries for a directory entry; unused for files.</summary>
    private readonly Dictionary<string, InMemoryVirtualFile> _children = new();

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public string Path { get; }

    /// <inheritdoc />
    public VirtualFileType Type { get; }

    /// <inheritdoc />
    public IVirtualFileSystem FileSystem { get; }

    /// <inheritdoc />
    public IVirtualFile? Parent { get; internal set; }

    /// <inheritdoc />
    public bool IsValid { get; internal set; } = true;

    /// <inheritdoc />
    public long Length => _content.LongLength;

    /// <inheritdoc />
    public DateTime LastModifiedUtc { get; internal set; }

    /// <summary>
    /// Creates a new in-memory virtual file or directory.
    /// </summary>
    /// <param name="name">The entry's simple name.</param>
    /// <param name="path">The entry's full virtual path.</param>
    /// <param name="type">Whether this is a file or a directory.</param>
    /// <param name="fileSystem">The owning file system.</param>
    /// <param name="parent">The parent directory, or <c>null</c> for a root entry.</param>
    public InMemoryVirtualFile(string name, string path, VirtualFileType type, IVirtualFileSystem fileSystem, IVirtualFile? parent)
    {
        Name = name;
        Path = path;
        Type = type;
        FileSystem = fileSystem;
        Parent = parent;
        _content = Array.Empty<byte>();
        LastModifiedUtc = DateTime.UtcNow;
    }

    /// <inheritdoc />
    public IReadOnlyList<IVirtualFile> GetChildren()
    {
        return new List<IVirtualFile>(_children.Values);
    }

    /// <inheritdoc />
    public Stream OpenRead()
    {
        if (Type != VirtualFileType.File)
        {
            throw new InvalidOperationException($"Cannot open a directory ('{Path}') for reading as a file.");
        }

        return new MemoryStream(_content, writable: false);
    }

    /// <summary>
    /// Replaces this file's content, updating its length and last-modified timestamp.
    /// Called only by <see cref="InMemoryFileSystem"/>, which owns write operations.
    /// </summary>
    /// <param name="newContent">The new content bytes.</param>
    internal void SetContent(byte[] newContent)
    {
        _content = newContent;
        LastModifiedUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Adds a child entry to this directory. Called only by <see cref="InMemoryFileSystem"/>.
    /// </summary>
    /// <param name="child">The child entry to add.</param>
    internal void AddChild(InMemoryVirtualFile child)
    {
        _children[child.Name] = child;
    }

    /// <summary>
    /// Removes a child entry from this directory by name. Called only by <see cref="InMemoryFileSystem"/>.
    /// </summary>
    /// <param name="childName">The name of the child to remove.</param>
    internal void RemoveChild(string childName)
    {
        _children.Remove(childName);
    }

    /// <summary>
    /// Looks up a direct child by name, without recursing into grandchildren.
    /// Used internally by <see cref="InMemoryFileSystem"/> when walking a path segment by segment.
    /// </summary>
    /// <param name="childName">The child's name.</param>
    /// <returns>The child entry, or <c>null</c> if no child with that name exists.</returns>
    internal InMemoryVirtualFile? FindChild(string childName)
    {
        return _children.TryGetValue(childName, out var child) ? child : null;
    }
}