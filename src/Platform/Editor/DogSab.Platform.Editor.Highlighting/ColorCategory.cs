namespace DogSab.Platform.Editor.Highlighting;

/// <summary>
/// A strongly-typed identifier for a category of syntax highlighting (e.g.
/// <c>"csharp.keyword"</c>, <c>"csharp.stringLiteral"</c>), wrapping a
/// stable string.
/// The same pattern already used for <c>VirtualFileSystemScheme</c>,
/// <c>Lexing.TokenType</c>, and <c>Tree.PsiElementType</c> elsewhere in the
/// platform — not an enum, since each language defines its own unbounded
/// set of highlighting categories, and a theme separately maps each
/// category to an actual color, so the platform itself never needs to
/// enumerate every possible category up front.
/// </summary>
public readonly struct ColorCategory : IEquatable<ColorCategory>
{
    /// <summary>
    /// The category's stable name, conventionally namespaced by language.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Creates a new color category identifier.
    /// </summary>
    /// <param name="value">
    /// The category's stable name.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="value"/> is null, empty, or whitespace.
    /// </exception>
    public ColorCategory(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Color category value must not be null or empty.", nameof(value));
        }

        Value = value;
    }
    
    /// <inheritdoc />
    public bool Equals(ColorCategory other) => string.Equals(Value, other.Value, StringComparison.Ordinal);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is ColorCategory other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    /// <inheritdoc />
    public override string ToString() => Value;

    /// <summary>
    /// Equality operator, delegating to <see cref="Equals(ColorCategory)"/>.
    /// </summary>
    public static bool operator ==(ColorCategory left, ColorCategory right) => left.Equals(right);

    /// <summary>
    /// Inequality operator, delegating to <see cref="Equals(ColorCategory)"/>.
    /// </summary>
    public static bool operator !=(ColorCategory left, ColorCategory right) => !left.Equals(right);

}