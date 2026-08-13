namespace DogSab.Platform.Indexing.Abstractions.Index;

/// <summary>
/// A strongly-typed identifier for a single declared index (e.g. "class name
/// index", "TODO comment index"), wrapping a stable string so index IDs
/// cannot be accidentally interchanged with unrelated string keys at call
/// sites — the same rationale as <c>PluginId</c>, <c>ModuleId</c>, and the
/// other identifier structs throughout the platform. Backed by a string
/// rather than a <see cref="Guid"/>, since — like <c>ModuleId</c> — indexes
/// are referred to by a stable, human-meaningful name declared once in code
/// (e.g. <c>"classNames"</c>), not generated fresh per instance.
/// </summary>
public readonly struct IndexId : IEquatable<IndexId>
{
    /// <summary>The index's stable name.</summary>
    public string Value { get; }
    
    /// <summary>
    /// Creates a new index identifier.
    /// </summary>
    /// <param name="value">The index's stable name.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is null, empty, or whitespace.</exception>
    public IndexId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Index id must not be null or empty.", nameof(value));
        }

        Value = value;
    }
    
    /// <inheritdoc/>
    public bool Equals(IndexId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
    
    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is IndexId other && Equals(other);
    
    /// <inheritdoc/>
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);
    
    /// <inheritdoc/>
    public override string ToString() => Value;
    
    /// <summary>Equality operator, delegating to <see cref="Equals(IndexId)"/>.</summary>
    public static bool operator ==(IndexId left, IndexId right) => left.Equals(right);

    /// <summary>Inequality operator, delegating to <see cref="Equals(IndexId)"/>.</summary>
    public static bool operator !=(IndexId left, IndexId right) => !left.Equals(right);
}