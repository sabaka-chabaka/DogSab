namespace DogSab.Platform.Editor.Abstractions.Caret;

/// <summary>
/// A position within a document, carrying both representations
/// simultaneously — the flat character <see cref="Offset"/> (convenient for
/// internal computation, and consistent with how <c>Psi.Abstractions.Tree.IPsiElement.StartOffset</c>
/// already works) and the <see cref="Line"/>/<see cref="Column"/> pair
/// (needed for rendering the caret and translating mouse clicks). Neither
/// representation is derived lazily from the other at each access — both are
/// computed once when the position is created (see
/// <see cref="Caret.ICaretModel"/> implementations, which own the
/// line-index used to compute this), avoiding repeated conversion for
/// something read as often as the caret position is.
/// </summary>
public readonly struct TextPosition : IEquatable<TextPosition>, IComparable<TextPosition>
{
    /// <summary>The flat character offset from the start of the document.</summary>
    public int Offset { get; }

    /// <summary>The zero-based line number.</summary>
    public int Line { get; }

    /// <summary>The zero-based column number within <see cref="Line"/>.</summary>
    public int Column { get; }

    /// <summary>
    /// Creates a new text position with both representations supplied
    /// directly. Callers are responsible for ensuring <paramref name="line"/>/<paramref name="column"/>
    /// are consistent with <paramref name="offset"/> for the document they
    /// apply to — typically only <see cref="Caret.ICaretModel"/> implementations
    /// construct these, having the document's line index available to
    /// compute both consistently.
    /// </summary>
    /// <param name="offset">The flat character offset.</param>
    /// <param name="line">The zero-based line number.</param>
    /// <param name="column">The zero-based column number.</param>
    public TextPosition(int offset, int line, int column)
    {
        Offset = offset;
        Line = line;
        Column = column;
    }

    /// <inheritdoc />
    public bool Equals(TextPosition other) => Offset == other.Offset;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is TextPosition other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => Offset;

    /// <summary>
    /// Compares positions by their flat offset — the only representation
    /// guaranteed to give a total, unambiguous order (comparing by line then
    /// column would require both positions to share a consistent line index,
    /// which offset comparison doesn't need).
    /// </summary>
    /// <param name="other">The position to compare against.</param>
    /// <returns>A negative value if this position is earlier, zero if equal, a positive value if later.</returns>
    public int CompareTo(TextPosition other) => Offset.CompareTo(other.Offset);

    /// <inheritdoc />
    public override string ToString() => $"{Line}:{Column} (offset {Offset})";

    /// <summary>Equality operator, delegating to <see cref="Equals(TextPosition)"/>.</summary>
    public static bool operator ==(TextPosition left, TextPosition right) => left.Equals(right);

    /// <summary>Inequality operator, delegating to <see cref="Equals(TextPosition)"/>.</summary>
    public static bool operator !=(TextPosition left, TextPosition right) => !left.Equals(right);

    /// <summary>Less-than operator, by offset.</summary>
    public static bool operator <(TextPosition left, TextPosition right) => left.CompareTo(right) < 0;

    /// <summary>Greater-than operator, by offset.</summary>
    public static bool operator >(TextPosition left, TextPosition right) => left.CompareTo(right) > 0;
}