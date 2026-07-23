namespace DogSab.Platform.Extensibility.Abstractions.Manifest;

/// <summary>
/// A strongly-typed identifier for a plugin, wrapping the stable string ID
/// declared in its manifest (e.g. <c>"dogsab.lang.csharp"</c>). Using a
/// dedicated value type instead of a bare <see cref="string"/> prevents plugin
/// IDs from being accidentally swapped with unrelated strings (extension point
/// IDs, file paths, display names) at call sites throughout the plugin system.
/// </summary>
public readonly struct PluginId : IEquatable<PluginId>
{
    /// <summary>The raw string identifier, as declared in the plugin's manifest.</summary>
    public string Value { get; }

    /// <summary>
    /// Creates a new plugin identifier.
    /// </summary>
    /// <param name="value">The raw string identifier, conventionally dot-separated and reverse-DNS-style (e.g. <c>"dogsab.lang.csharp"</c>).</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is null, empty, or whitespace.</exception>
    public PluginId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Plugin id must not be null or empty.", nameof(value));
        }

        Value = value;
    }

    /// <inheritdoc />
    public bool Equals(PluginId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is PluginId other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    /// <inheritdoc />
    public override string ToString() => Value;

    /// <summary>Equality operator, delegating to <see cref="Equals(PluginId)"/>.</summary>
    public static bool operator ==(PluginId left, PluginId right) => left.Equals(right);

    /// <summary>Inequality operator, delegating to <see cref="Equals(PluginId)"/>.</summary>
    public static bool operator !=(PluginId left, PluginId right) => !left.Equals(right);
}