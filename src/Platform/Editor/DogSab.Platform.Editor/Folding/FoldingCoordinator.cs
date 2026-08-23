using DogSab.Platform.Editor.Abstractions.Events;
using DogSab.Platform.Editor.Abstractions.Folding;
using DogSab.Platform.Extensibility.Abstractions.ExtensionPoints;
using DogSab.Platform.Psi.Abstractions.Tree;

namespace DogSab.Platform.Editor.Folding;

/// <summary>
/// Resolves and invokes the correct <see cref="IFoldingProvider"/> for a
/// file's language, computing its foldable regions.
/// Every registered folding provider is asked to contribute regions rather
/// than picking a single "best" provider for the file's language, since a
/// language could plausibly have more than one folding provider
/// contributing different kinds of regions (e.g. one for method bodies,
/// another for region comments), and there is no platform-level way to
/// know in advance which one is "the" provider for a given language.
/// </summary>
public sealed class FoldingCoordinator
{
    /// <summary>
    /// The platform's extension point registry, used to look up every
    /// currently registered <see cref="IFoldingProvider"/>.
    /// </summary>
    private readonly IExtensionPointRegistry _extensionPointRegistry;

    /// <summary>
    /// Creates a new folding coordinator.
    /// </summary>
    /// <param name="extensionPointRegistry">
    /// The registry to resolve folding providers from.
    /// </param>
    public FoldingCoordinator(IExtensionPointRegistry extensionPointRegistry)
    {
        _extensionPointRegistry = extensionPointRegistry;
    }

    /// <summary>
    /// Computes every foldable region for a file by asking every currently
    /// registered folding provider to contribute its findings.
    /// </summary>
    /// <param name="psiFile">
    /// The file's parsed PSI tree to compute folding regions for.
    /// </param>
    /// <returns>
    /// Every foldable region found, across all registered providers,
    /// in no particular guaranteed order.
    /// </returns>
    public IReadOnlyList<IFoldingRegion> ComputeFoldingRegions(IPsiFile psiFile)
    {
        var results = new List<IFoldingRegion>();

        foreach (var provider in _extensionPointRegistry.GetExtensions(EditorExtensionPoints.FOLDING_PROVIDER))
        {
            results.AddRange(provider.ComputeFoldingRegions(psiFile));
        }

        return results;
    }
}