namespace DogSab.Platform.Vfs.Abstractions.Exceptions;

/// <summary>
/// Thrown when code requests a virtual file at a path that does not exist,
/// in contexts where the caller expects the file to be present rather than
/// checking for <c>null</c> from <see cref="FileSystem.IVirtualFileSystem.FindFile"/> itself.
/// </summary>
public sealed class VirtualFileNotFoundException : Exception
{
    /// <summary>The virtual path that could not be resolved to an existing file.</summary>
    public string Path { get; }

    /// <summary>
    /// Creates a new exception describing a missing virtual file.
    /// </summary>
    /// <param name="path">The virtual path that could not be resolved.</param>
    public VirtualFileNotFoundException(string path)
        : base($"No virtual file exists at path '{path}'.")
    {
        Path = path;
    }
}