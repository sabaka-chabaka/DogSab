using DogSab.Platform.Editor.Abstractions.Caret;
using DogSab.Platform.Psi.Abstractions.Tree;

namespace DogSab.Platform.Editor.Abstractions.Completion;

/// <summary>
/// Provides code completion suggestions for a specific language — this is
/// the concrete contract behind the running example used throughout this
/// project's design discussions ("editor.completionContributor").
/// Registered against <see cref="Events.EditorExtensionPoints.COMPLETION_PROVIDER"/>.
/// </summary>
public interface ICompletionProvider
{
    /// <summary>
    /// Computes completion suggestions applicable at a given caret position
    /// within a parsed file.
    /// </summary>
    /// <param name="psiFile">The file's parsed PSI tree.</param>
    /// <param name="caretPosition">The position completion was requested at.</param>
    /// <returns>The suggested completion items, in no particular order — the Editor's popup applies its own sort by <see cref="CompletionItem.Priority"/>.</returns>
    IEnumerable<CompletionItem> GetCompletions(IPsiFile psiFile, TextPosition caretPosition);
}