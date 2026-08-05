using System.Collections.Concurrent;
using DogSab.Platform.Vfs.Abstractions.Exceptions;
using DogSab.Platform.Vfs.Abstractions.FileSystem;

namespace DogSab.Platform.Vfs.FileSystem;

/// <summary>
/// Holds the mapping from scheme string (e.g. <c>"file"</c>) to the
/// <see cref="IVirtualFileSystem"/> provider that handles it. Registration is
/// kept separate from path resolution (<see cref="VirtualFileSystemRouter"/>)
/// so that new providers — including ones contributed by plugins, e.g. a
/// future provider for browsing Docker image layers — can register
/// themselves without needing to know anything about how paths get routed to
/// them; they only need to know their own scheme string.
/// </summary>
public sealed class VirtualFileSystemRegistry
{
    /// <summary>Registered providers, keyed by the scheme they handle.</summary>
    private readonly ConcurrentDictionary<string, IVirtualFileSystem> _providersByScheme = new();

    /// <summary>
    /// Registers a file system provider for its declared scheme.
    /// </summary>
    /// <param name="fileSystem">The provider to register, identified by its own <see cref="IVirtualFileSystem.Scheme"/>.</param>
    /// <exception cref="System.InvalidOperationException">
    /// Thrown if a provider is already registered for the same scheme.
    /// </exception>
    public void Register(IVirtualFileSystem fileSystem)
    {
        if (!_providersByScheme.TryAdd(fileSystem.Scheme, fileSystem))
        {
            throw new System.InvalidOperationException(
                $"A file system provider is already registered for scheme '{fileSystem.Scheme}'.");
        }
    }

    /// <summary>
    /// Removes the provider registered for a scheme, if any.
    /// </summary>
    /// <param name="scheme">The scheme to unregister.</param>
    /// <returns><c>true</c> if a provider was found and removed; otherwise <c>false</c>.</returns>
    public bool Unregister(string scheme)
    {
        return _providersByScheme.TryRemove(scheme, out _);
    }

    /// <summary>
    /// Resolves the provider registered for a scheme.
    /// </summary>
    /// <param name="scheme">The scheme to look up.</param>
    /// <returns>The registered provider.</returns>
    /// <exception cref="UnsupportedFileSystemSchemeException">Thrown if no provider is registered for <paramref name="scheme"/>.</exception>
    public IVirtualFileSystem Resolve(string scheme)
    {
        if (!_providersByScheme.TryGetValue(scheme, out var provider))
        {
            throw new UnsupportedFileSystemSchemeException(scheme);
        }

        return provider;
    }

    /// <summary>
    /// Checks whether a provider is registered for a scheme.
    /// </summary>
    /// <param name="scheme">The scheme to check.</param>
    /// <returns><c>true</c> if a provider is registered; otherwise <c>false</c>.</returns>
    public bool IsRegistered(string scheme)
    {
        return _providersByScheme.ContainsKey(scheme);
    }
}