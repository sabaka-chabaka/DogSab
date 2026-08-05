using DogSab.Platform.Vfs.Abstractions.VirtualFile;
using DogSab.Platform.Vfs.VirtualFile;

namespace DogSab.Platform.Vfs.FileSystem;

/// <summary>
/// The platform's single public entry point for resolving a full virtual path
/// to an <see cref="IVirtualFile"/>. Parses the path's scheme via
/// <see cref="VirtualFilePathParser"/> and delegates to whichever provider is
/// registered for that scheme in <see cref="VirtualFileSystemRegistry"/>.
/// Platform code should depend on this router, not on
/// <see cref="VirtualFileSystemRegistry"/> directly — the registry is the
/// registration-time concern (who provides what), the router is the
/// read-time concern (resolve me a file, I don't care which provider it came from).
/// </summary>
public sealed class VirtualFileSystemRouter
{
    /// <summary>The registry this router resolves providers from.</summary>
    private readonly VirtualFileSystemRegistry _registry;

    /// <summary>
    /// Creates a new router.
    /// </summary>
    /// <param name="registry">The registry to resolve providers from.</param>
    public VirtualFileSystemRouter(VirtualFileSystemRegistry registry)
    {
        _registry = registry;
    }

    /// <summary>
    /// Resolves a full virtual path to a virtual file, if it exists.
    /// </summary>
    /// <param name="fullPath">The full virtual path, including scheme (e.g. <c>"file:///home/user/x.cs"</c>).</param>
    /// <returns>The resolved file, or <c>null</c> if nothing exists at that path.</returns>
    /// <exception cref="Abstractions.Exceptions.UnsupportedFileSystemSchemeException">
    /// Thrown if no provider is registered for the path's scheme.
    /// </exception>
    /// <exception cref="System.FormatException">Thrown if <paramref name="fullPath"/> is not a validly formed virtual path.</exception>
    public IVirtualFile? FindFile(string fullPath)
    {
        var parsed = VirtualFilePathParser.Parse(fullPath);
        var provider = _registry.Resolve(parsed.Scheme);

        return provider.FindFile(fullPath);
    }

    /// <summary>
    /// Resolves a full virtual path, throwing if the file does not exist,
    /// for callers that expect the file to be present rather than handling
    /// a <c>null</c> result themselves.
    /// </summary>
    /// <param name="fullPath">The full virtual path to resolve.</param>
    /// <returns>The resolved file.</returns>
    /// <exception cref="Abstractions.Exceptions.VirtualFileNotFoundException">Thrown if no file exists at <paramref name="fullPath"/>.</exception>
    public IVirtualFile Require(string fullPath)
    {
        return FindFile(fullPath) ?? throw new Abstractions.Exceptions.VirtualFileNotFoundException(fullPath);
    }
}