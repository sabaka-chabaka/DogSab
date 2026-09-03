using DogSab.Platform.Core.Abstractions.Logging;
using DogSab.Platform.Editor.Abstractions.Caret;
using DogSab.Platform.Editor.Abstractions.Completion;
using DogSab.Platform.Editor.Abstractions.Events;
using DogSab.Platform.Editor.Abstractions.Folding;
using DogSab.Platform.Extensibility.Abstractions.ExtensionPoints;
using DogSab.Platform.Psi.Abstractions.Tree;

namespace DogSab.Platform.Editor.Completion;

/// <summary>
/// Resolves and invokes every registered <see cref="ICompletionProvider"/>
/// for a file, aggregating and sorting their suggestions into a single
/// ranked list.
/// Unlike <see cref="Folding.FoldingCoordinator"/>, the aggregated results
/// here are explicitly sorted by
/// <see cref="CompletionItem.Priority"/> before being returned, since
/// completion suggestions are directly presented to the user in a ranked
/// popup list, where display order matters — folding regions have no such
/// user-facing ordering concern.
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