namespace DogSab.Platform.Editor.Highlighting;

/// <summary>
/// A single range of a document's text assigned a specific highlighting
/// category, e.g. characters 10 through 16 categorized as
/// <c>"csharp.keyword"</c>.
/// </summary>
public readonly struct HighlightSpan
{
    /// <summary>
    /// The character offset where the highlighted range starts.
    /// </summary>
    public int StartOffset { get; }

    /// <summary>
    /// The length, in characters, of the highlighted range.
    /// </summary>
    public int Length { get; }

    /// <summary>
    /// The highlighting category assigned to this range.
    /// </summary>
    public ColorCategory Category { get; }

    /// <summary>
    /// Creates a new highlight span.
    /// </summary>
    /// <param name="startOffset">
    /// The character offset where the range starts.
    /// </param>
    /// <param name="length">
    /// The length, in characters, of the range.
    /// </param>
    /// <param name="category">
    /// The highlighting category to assign.
    /// </param>
    public HighlightSpan(int startOffset, int length, ColorCategory category)
    {
        StartOffset = startOffset;
        Length = length;
        Category = category;
    }
}