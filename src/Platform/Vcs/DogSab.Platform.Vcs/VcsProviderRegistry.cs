using DogSab.Platform.Extensibility.Abstractions.ExtensionPoints;
using DogSab.Platform.Vcs.Abstractions;
using DogSab.Platform.Vcs.Abstractions.Events;
using DogSab.Platform.Vfs.Abstractions.VirtualFile;

namespace DogSab.Platform.Vcs;

/// <summary>
/// Resolves which registered <see cref="IVcsProvider"/> is responsible for
/// a given directory, by asking each registered provider whether it
/// recognizes/owns that directory via <see cref="IVcsProvider.OwnsDirectory"/>.
/// Distinct from a typical platform registry (like
/// <c>Extensibility.Registry.ExtensionPointRegistryImpl</c>) in that
/// resolution here is not a simple ID lookup — it requires actually asking
/// each candidate provider a question about the specific directory, since
/// which VCS (if any) manages a given folder is a fact about the
/// filesystem, not something declared upfront by ID.
/// </summary>
public sealed class VcsProviderRegistry
{
    /// <summary>
    /// The platform's extension point registry, used to look up every
    /// currently registered <see cref="IVcsProvider"/>.
    /// </summary>
    private readonly IExtensionPointRegistry _extensionPointRegistry;

    /// <summary>
    /// Creates a new VCS provider registry.
    /// </summary>
    /// <param name="extensionPointRegistry">
    /// The registry to resolve VCS providers from.
    /// </param>
    public VcsProviderRegistry(IExtensionPointRegistry extensionPointRegistry)
    {
        _extensionPointRegistry = extensionPointRegistry;
    }

    /// <summary>
    /// Resolves which registered provider, if any, owns a given directory.
    /// If more than one provider claims ownership (an unusual but not
    /// impossible configuration, e.g. nested repositories under two
    /// different VCS kinds), the first one found is returned — providers
    /// are not expected to overlap in practice, so no attempt is made to
    /// pick the "most specific" one among conflicting claims.
    /// </summary>
    /// <param name="directory">
    /// The directory to resolve a provider for.
    /// </param>
    /// <returns>
    /// The owning provider, or <c>null</c> if no registered provider
    /// claims this directory (e.g. the folder isn't under any version
    /// control at all).
    /// </returns>
    public IVcsProvider? ResolveProvider(IVirtualFile directory)
    {
        return _extensionPointRegistry
            .GetExtensions(VcsExtensionPoints.VCS_PROVIDER)
            .FirstOrDefault(provider => provider.OwnsDirectory(directory));
    }
}