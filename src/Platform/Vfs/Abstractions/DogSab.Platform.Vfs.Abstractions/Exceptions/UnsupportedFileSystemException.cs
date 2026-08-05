namespace DogSab.Platform.Vfs.Abstractions.Exceptions;

/// <summary>
/// Thrown when a virtual path references a scheme (e.g. <c>"custom://"</c>)
/// for which no <see cref="FileSystem.IVirtualFileSystem"/> provider is
/// registered with the platform.
/// </summary>
public sealed class UnsupportedFileSystemSchemeException : Exception
{
    /// <summary>The unrecognized scheme extracted from the requested path.</summary>
    public string Scheme { get; }

    /// <summary>
    /// Creates a new exception describing an unsupported file system scheme.
    /// </summary>
    /// <param name="scheme">The unrecognized scheme.</param>
    public UnsupportedFileSystemSchemeException(string scheme)
        : base($"No file system provider is registered for scheme '{scheme}'.")
    {
        Scheme = scheme;
    }
}