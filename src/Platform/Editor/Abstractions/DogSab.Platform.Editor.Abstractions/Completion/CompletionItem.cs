namespace DogSab.Platform.Editor.Abstractions.Completion;

/// <summary>
/// A single suggested completion offered to the user while typing (e.g. a
/// method name, a keyword, a variable in scope).
/// </summary>
public readonly struct CompletionItem
{
    /// <summary>The text inserted into the document when this item is chosen.</summary>
    public string InsertText { get; }

    /// <summary>The text shown in the completion popup list (may differ from <see cref="InsertText"/>, e.g. showing a method's full signature while inserting just its name).</summary>
    public string DisplayText { get; }

    /// <summary>A short description shown alongside the item (e.g. the symbol's declared type), or empty if none.</summary>
    public string Detail { get; }

    /// <summary>
    /// A relative priority used to order items in the popup — higher values
    /// sort first. Left as a plain <see cref="int"/> rather than an enum,
    /// since ranking heuristics (e.g. weighting by recency of use, scope
    /// proximity) are language- and provider-specific and not meaningfully
    /// enumerable by the platform.
    /// </summary>
    public int Priority { get; }

    /// <summary>
    /// Creates a new completion item.
    /// </summary>
    /// <param name="insertText">The text inserted when chosen.</param>
    /// <param name="displayText">The text shown in the popup. Defaults to <paramref name="insertText"/> if not supplied.</param>
    /// <param name="detail">A short description shown alongside the item.</param>
    /// <param name="priority">The relative sort priority. Defaults to 0.</param>
    public CompletionItem(string insertText, string? displayText = null, string detail = "", int priority = 0)
    {
        InsertText = insertText;
        DisplayText = displayText ?? insertText;
        Detail = detail;
        Priority = priority;
    }
}