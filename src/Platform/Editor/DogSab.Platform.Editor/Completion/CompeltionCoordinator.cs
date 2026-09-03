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
public sealed class CompletionCoordinator
{
    private readonly IExtensionPointRegistry _extensionPointRegistry;
    private readonly ILogger _logger;

    public CompletionCoordinator(IExtensionPointRegistry extensionPointRegistry, ILoggerFactory loggerFactory)
    {
        _extensionPointRegistry = extensionPointRegistry;
        _logger = loggerFactory.GetLogger(typeof(CompletionCoordinator));
    }

    public IReadOnlyList<CompletionItem> GetCompletions(IPsiFile psiFile, TextPosition caretPosition)
    {
        var results = new List<CompletionItem>();

        foreach (var provider in _extensionPointRegistry.GetExtensions(EditorExtensionPoints.COMPLETION_PROVIDER))
        {
            try
            {
                results.AddRange(provider.GetCompletions(psiFile, caretPosition));
            }
            catch (Exception ex)
            {
                _logger.Error("Completion provider '{0}' failed for file '{1}'", ex, provider.GetType().FullName, psiFile.VirtualFile.Path);
            }
        }

        return results.OrderByDescending(item => item.Priority).ToList();
    }
}