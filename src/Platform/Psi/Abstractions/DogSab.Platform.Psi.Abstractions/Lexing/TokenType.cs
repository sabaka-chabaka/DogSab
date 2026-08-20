namespace DogSab.Platform.Psi.Abstractions.Lexing;

/// <summary>
/// A strongly-typed identifier for a kind of lexical token (e.g.
/// <c>"csharp.identifier"</c>, <c>"csharp.keyword.class"</c>), wrapping a
/// stable string. Not an enum, since each language defines its own,
/// unbounded set of token kinds unknown to the platform at compile time —
/// the same reasoning as <see cref="Vfs.Abstractions.FileSystem.VirtualFileSystemScheme"/>
/// being string constants rather than an enum. Token type identity is by
/// value (two <see cref="TokenType"/> instances with the same
/// <see cref="Value"/> are equal), unlike <see cref="Language.LanguageId"/>'s
/// sibling structs — this matters because generic/shared platform code (e.g.
/// syntax highlighting) may compare token types produced by a language
/// plugin against well-known constants without holding the exact same object reference.
/// </summary>
public readonly struct TokenType : IEquatable<TokenType>
{
    /// <summary>The token kind's stable name, conventionally namespaced by language (e.g. <c>"csharp.identifier"</c>).</summary>
    public string Value { get; }

    /// <summary>
    /// Creates a new token type identifier.
    /// </summary>
    /// <param name="value">The token kind's stable name.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is null, empty, or whitespace.</exception>
    public TokenType(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Token type value must not be null or empty.", nameof(value));
        }

        Value = value;
    }

    /// <inheritdoc />
    public bool Equals(TokenType other) => string.Equals(Value, other.Value, StringComparison.Ordinal);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is TokenType other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    /// <inheritdoc />
    public override string ToString() => Value;

    /// <summary>Equality operator, delegating to <see cref="Equals(TokenType)"/>.</summary>
    public static bool operator ==(TokenType left, TokenType right) => left.Equals(right);

    /// <summary>Inequality operator, delegating to <see cref="Equals(TokenType)"/>.</summary>
    public static bool operator !=(TokenType left, TokenType right) => !left.Equals(right);
}