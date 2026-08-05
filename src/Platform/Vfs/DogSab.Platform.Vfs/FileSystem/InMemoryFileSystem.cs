using DogSab.Platform.Vfs.Abstractions.FileSystem;
using DogSab.Platform.Vfs.Abstractions.VirtualFile;
using DogSab.Platform.Vfs.VirtualFile;

namespace DogSab.Platform.Vfs.FileSystem;

/// <summary>
/// An <see cref="IVirtualFileSystem"/> backed entirely by in-process memory.
/// Maintains its own root directory tree of <see cref="InMemoryVirtualFile"/>
/// entries; nothing is ever read from or written to disk. Useful for
/// generated content, scratch files, and tests that need a working file
/// system without touching the real disk.
/// </summary>
public sealed class InMemoryFileSystem : IVirtualFileSystem
{
    /// <summary>The single root directory entry all paths are resolved relative to.</summary>
    private readonly InMemoryVirtualFile _root;

    /// <summary>
    /// Creates a new, empty in-memory file system.
    /// </summary>
    public InMemoryFileSystem()
    {
        _root = new InMemoryVirtualFile("", VirtualFilePathParser.Combine(Scheme, "/"), VirtualFileType.Directory, this, parent: null);
    }

    /// <inheritdoc />
    public string Scheme => VirtualFileSystemScheme.InMemory;

    /// <inheritdoc />
    public bool IsWritable => true;

    /// <inheritdoc />
    public IVirtualFile? FindFile(string fullPath)
    {
        var parsed = VirtualFilePathParser.Parse(fullPath);
        return WalkToEntry(parsed.Path);
    }

    /// <inheritdoc />
    public Task WriteAsync(IVirtualFile file, Stream content, CancellationToken cancellationToken)
    {
        var entry = RequireOwnEntry(file);

        using var memoryStream = new MemoryStream();
        content.CopyTo(memoryStream);
        entry.SetContent(memoryStream.ToArray());

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<IVirtualFile> CreateFileAsync(IVirtualFile directory, string fileName, Stream content, CancellationToken cancellationToken)
    {
        var directoryEntry = RequireOwnEntry(directory);

        if (directoryEntry.Type != VirtualFileType.Directory)
        {
            throw new InvalidOperationException($"Cannot create a file under '{directory.Path}': it is not a directory.");
        }

        var newPath = VirtualFilePathParser.Combine(Scheme, CombineSegments(GetRelativePath(directoryEntry), fileName));
        var newEntry = new InMemoryVirtualFile(fileName, newPath, VirtualFileType.File, this, directoryEntry);

        using var memoryStream = new MemoryStream();
        content.CopyTo(memoryStream);
        newEntry.SetContent(memoryStream.ToArray());

        directoryEntry.AddChild(newEntry);

        return Task.FromResult<IVirtualFile>(newEntry);
    }

    /// <inheritdoc />
    public Task DeleteAsync(IVirtualFile file, CancellationToken cancellationToken)
    {
        var entry = RequireOwnEntry(file);

        if (entry.Parent is InMemoryVirtualFile parentEntry)
        {
            parentEntry.RemoveChild(entry.Name);
        }

        entry.IsValid = false;

        return Task.CompletedTask;
    }

    /// <summary>
    /// Walks the in-memory tree segment by segment to resolve a relative path
    /// (without scheme) to an entry.
    /// </summary>
    /// <param name="relativePath">The path portion (e.g. <c>"/foo/bar.txt"</c>), without scheme.</param>
    /// <returns>The resolved entry, or <c>null</c> if any segment along the way does not exist.</returns>
    private InMemoryVirtualFile? WalkToEntry(string relativePath)
    {
        var segments = relativePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var current = _root;

        foreach (var segment in segments)
        {
            var next = current.FindChild(segment);
            if (next is null)
            {
                return null;
            }

            current = next;
        }

        return current;
    }

    /// <summary>
    /// Verifies that a given <see cref="IVirtualFile"/> actually belongs to
    /// this file system instance and casts it to the concrete
    /// <see cref="InMemoryVirtualFile"/> type needed for mutation.
    /// </summary>
    /// <param name="file">The file to verify and cast.</param>
    /// <returns>The same entry, typed as <see cref="InMemoryVirtualFile"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown if <paramref name="file"/> was not created by this file system.</exception>
    private static InMemoryVirtualFile RequireOwnEntry(IVirtualFile file)
    {
        return file as InMemoryVirtualFile
            ?? throw new InvalidOperationException(
                $"File '{file.Path}' does not belong to this {nameof(InMemoryFileSystem)} instance.");
    }

    /// <summary>
    /// Derives a directory entry's path relative to the root, for building child paths.
    /// </summary>
    /// <param name="entry">The directory entry.</param>
    /// <returns>The relative path portion, without scheme.</returns>
    private string GetRelativePath(InMemoryVirtualFile entry)
    {
        var parsed = VirtualFilePathParser.Parse(entry.Path);
        return parsed.Path;
    }

    /// <summary>
    /// Joins a directory's relative path with a child name into a new relative path.
    /// </summary>
    /// <param name="directoryRelativePath">The parent directory's relative path.</param>
    /// <param name="childName">The child entry's name.</param>
    /// <returns>The combined relative path.</returns>
    private static string CombineSegments(string directoryRelativePath, string childName)
    {
        return directoryRelativePath.TrimEnd('/') + "/" + childName;
    }
}