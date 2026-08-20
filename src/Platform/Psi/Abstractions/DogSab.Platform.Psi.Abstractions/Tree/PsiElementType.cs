namespace DogSab.Platform.Psi.Abstractions.Tree;

/// <summary>
/// A strongly-typed identifier for a kind of PSI tree node (e.g.
/// <c>"csharp.classDeclaration"</c>, <c>"csharp.methodDeclaration"</c>).
/// Same rationale and equality semantics as <see cref="Lexing.TokenType"/> —
/// a string-backed, value-equal identifier rather than an enum, since each
/// language defines its own unbounded set of node kinds.
/// </summary>
public readonly struct PsiElementType : IEquatable<PsiElementType>
{
    /// <summary>The node kind's stable name, conventionally namespaced by language.</summary>
    public string Value { get; }

    /// <summary>
    /// Creates a new PSI element type identifier.
    /// </summary>
    /// <param name="value">The node kind's stable name.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is null, empty, or whitespace.</exception>
    public PsiElementType(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("PSI element type value must not be null or empty.", nameof(value));
        }

        Value = value;
    }

    /// <inheritdoc />
    public bool Equals(PsiElementType other) => string.Equals(Value, other.Value, StringComparison.Ordinal);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is PsiElementType other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    /// <inheritdoc />
    public override string ToString() => Value;

    /// <summary>Equality operator, delegating to <see cref="Equals(PsiElementType)"/>.</summary>
    public static bool operator ==(PsiElementType left, PsiElementType right) => left.Equals(right);

    /// <summary>Inequality operator, delegating to <see cref="Equals(PsiElementType)"/>.</summary>
    public static bool operator !=(PsiElementType left, PsiElementType right) => !left.Equals(right);
}