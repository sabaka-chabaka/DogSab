using DogSab.Platform.Core.Abstractions.Logging;
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
    private readonly IExtensionPointRegistry _extensionPointRegistry;
    private readonly ILogger _logger;

    public FoldingCoordinator(IExtensionPointRegistry extensionPointRegistry, ILoggerFactory loggerFactory)
    {
        _extensionPointRegistry = extensionPointRegistry;
        _logger = loggerFactory.GetLogger(typeof(FoldingCoordinator));
    }

    public IReadOnlyList<IFoldingRegion> ComputeFoldingRegions(IPsiFile psiFile)
    {
        var results = new List<IFoldingRegion>();

        foreach (var provider in _extensionPointRegistry.GetExtensions(EditorExtensionPoints.FOLDING_PROVIDER))
        {
            try
            {
                results.AddRange(provider.ComputeFoldingRegions(psiFile));
            }
            catch (Exception ex)
            {
                _logger.Error("Folding provider '{0}' failed for file '{1}'", ex, provider.GetType().FullName, psiFile.VirtualFile.Path);
            }
        }

        return results;
    }
}