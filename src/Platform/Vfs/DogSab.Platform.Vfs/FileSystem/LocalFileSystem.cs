using DogSab.Platform.Vfs.Abstractions.FileSystem;
using DogSab.Platform.Vfs.Abstractions.VirtualFile;
using DogSab.Platform.Vfs.VirtualFile;

namespace DogSab.Platform.Vfs.FileSystem;

/// <summary>
/// An <see cref="IVirtualFileSystem"/> backed by the real local disk.
/// Translates between the platform's virtual path format
/// (<c>"file:///home/user/x.cs"</c>) and real OS paths
/// (<c>"/home/user/x.cs"</c> on Unix, <c>"C:\Users\..."</c> on Windows),
/// delegating all actual I/O to <see cref="System.IO"/>.
/// </summary>
public sealed class LocalFileSystem : IVirtualFileSystem
{
    /// <inheritdoc />
    public string Scheme => VirtualFileSystemScheme.Local;

    /// <inheritdoc />
    public bool IsWritable => true;

    /// <inheritdoc />
    public IVirtualFile? FindFile(string fullPath)
    {
        var diskPath = ToDiskPath(fullPath);

        if (!File.Exists(diskPath) && !Directory.Exists(diskPath))
        {
            return null;
        }

        var parentDiskPath = Path.GetDirectoryName(diskPath);
        IVirtualFile? parent = null;

        if (parentDiskPath is not null && (File.Exists(parentDiskPath) || Directory.Exists(parentDiskPath)))
        {
            var parentVirtualPath = ToVirtualPath(parentDiskPath);
            parent = new LocalVirtualFile(parentDiskPath, parentVirtualPath, this, null);
        }

        return new LocalVirtualFile(diskPath, fullPath, this, parent);
    }

    /// <inheritdoc />
    public async Task WriteAsync(IVirtualFile file, Stream content, CancellationToken cancellationToken)
    {
        var localFile = RequireOwnEntry(file);

        await using var fileStream = new FileStream(localFile.DiskPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await content.CopyToAsync(fileStream, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<IVirtualFile> CreateFileAsync(IVirtualFile directory, string fileName, Stream content, CancellationToken cancellationToken)
    {
        var localDirectory = RequireOwnEntry(directory);

        if (localDirectory.Type != VirtualFileType.Directory)
        {
            throw new InvalidOperationException($"Cannot create a file under '{directory.Path}': it is not a directory.");
        }

        var newDiskPath = Path.Combine(localDirectory.DiskPath, fileName);

        await using (var fileStream = new FileStream(newDiskPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
        {
            await content.CopyToAsync(fileStream, cancellationToken).ConfigureAwait(false);
        }

        var newVirtualPath = ToVirtualPath(newDiskPath);
        return new LocalVirtualFile(newDiskPath, newVirtualPath, this, directory);
    }

    /// <inheritdoc />
    public Task DeleteAsync(IVirtualFile file, CancellationToken cancellationToken)
    {
        var localFile = RequireOwnEntry(file);

        if (localFile.Type == VirtualFileType.Directory)
        {
            Directory.Delete(localFile.DiskPath, recursive: true);
        }
        else
        {
            File.Delete(localFile.DiskPath);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Converts a full virtual path (with <c>"file://"</c> scheme) to a real OS disk path.
    /// </summary>
    /// <param name="fullPath">The full virtual path.</param>
    /// <returns>The corresponding OS disk path.</returns>
    private static string ToDiskPath(string fullPath)
    {
        var parsed = VirtualFilePathParser.Parse(fullPath);
        return parsed.Path.Replace('/', Path.DirectorySeparatorChar);
    }

    /// <summary>
    /// Converts a real OS disk path to this file system's full virtual path form.
    /// </summary>
    /// <param name="diskPath">The OS disk path.</param>
    /// <returns>The corresponding full virtual path.</returns>
    private string ToVirtualPath(string diskPath)
    {
        var normalizedPath = diskPath.Replace(Path.DirectorySeparatorChar, '/');
        return VirtualFilePathParser.Combine(Scheme, normalizedPath);
    }

    /// <summary>
    /// Verifies a given <see cref="IVirtualFile"/> belongs to this file system
    /// and casts it to the concrete <see cref="LocalVirtualFile"/> type needed for I/O.
    /// </summary>
    /// <param name="file">The file to verify and cast.</param>
    /// <returns>The same entry, typed as <see cref="LocalVirtualFile"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown if <paramref name="file"/> was not created by this file system.</exception>
    private static LocalVirtualFile RequireOwnEntry(IVirtualFile file)
    {
        return file as LocalVirtualFile
            ?? throw new InvalidOperationException(
                $"File '{file.Path}' does not belong to this {nameof(LocalFileSystem)} instance.");
    }
}