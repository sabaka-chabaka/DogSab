namespace DogSab.Platform.Psi.Abstractions.Language;

/// <summary>
/// A strongly-typed identifier for a programming language (e.g. <c>"csharp"</c>,
/// <c>"python"</c>), wrapping a stable string so language IDs cannot be
/// accidentally interchanged with unrelated string keys — the same pattern
/// as <c>PluginId</c>, <c>ModuleId</c>, and <c>IndexId</c> elsewhere in the platform.
/// </summary>
public readonly struct LanguageId : IEquatable<LanguageId>
{
    /// <summary>The language's stable name.</summary>
    public string Value { get; }

    /// <summary>
    /// Creates a new language identifier.
    /// </summary>
    /// <param name="value">The language's stable name.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is null, empty, or whitespace.</exception>
    public LanguageId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Language id must not be null or empty.", nameof(value));
        }

        Value = value;
    }
    
    /// <inheritdoc />
    public bool Equals(LanguageId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is LanguageId other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    /// <inheritdoc />
    public override string ToString() => Value;

    /// <summary>Equality operator, delegating to <see cref="Equals(LanguageId)"/>.</summary>
    public static bool operator ==(LanguageId left, LanguageId right) => left.Equals(right);

    /// <summary>Inequality operator, delegating to <see cref="Equals(LanguageId)"/>.</summary>
    public static bool operator !=(LanguageId left, LanguageId right) => !left.Equals(right);
}