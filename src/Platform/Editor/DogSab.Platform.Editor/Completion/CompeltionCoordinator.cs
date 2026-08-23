using DogSab.Platform.Editor.Abstractions.Caret;
using DogSab.Platform.Editor.Abstractions.Completion;
using DogSab.Platform.Editor.Abstractions.Events;
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
    /// <summary>
    /// The platform's extension point registry, used to look up every
    /// currently registered <see cref="ICompletionProvider"/>.
    /// </summary>
    private readonly IExtensionPointRegistry _extensionPointRegistry;

    /// <summary>
    /// Creates a new completion coordinator.
    /// </summary>
    /// <param name="extensionPointRegistry">
    /// The registry to resolve completion providers from.
    /// </param>
    public CompletionCoordinator(IExtensionPointRegistry extensionPointRegistry)
    {
        _extensionPointRegistry = extensionPointRegistry;
    }

    /// <summary>
    /// Computes completion suggestions at a given position by asking every
    /// currently registered completion provider to contribute its
    /// findings, then sorting the aggregated result by descending priority.
    /// </summary>
    /// <param name="psiFile">
    /// The file's parsed PSI tree.
    /// </param>
    /// <param name="caretPosition">
    /// The position completion was requested at.
    /// </param>
    /// <returns>
    /// The aggregated completion items, ordered from highest to lowest
    /// <see cref="CompletionItem.Priority"/>.
    /// </returns>
    public IReadOnlyList<CompletionItem> GetCompletions(IPsiFile psiFile, TextPosition caretPosition)
    {
        var results = new List<CompletionItem>();

        foreach (var provider in _extensionPointRegistry.GetExtensions(EditorExtensionPoints.COMPLETION_PROVIDER))
        {
            results.AddRange(provider.GetCompletions(psiFile, caretPosition));
        }

        return results
            .OrderByDescending(item => item.Priority)
            .ToList();
    }
}