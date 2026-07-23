namespace DogSab.Platform.Extensibility.Abstractions.ExtensionPoints;

/// <summary>
/// A typed identifier for an extension point: a named "slot" the platform
/// declares, into which plugins register implementations of
/// <typeparamref name="TContract"/>. Identity is by <see cref="Id"/> (a stable
/// string, e.g. <c>"editor.completionContributor"</c>) rather than by object
/// reference, since plugins declare their extensions declaratively in a JSON
/// manifest and must be able to refer to an extension point without a
/// compile-time reference to the platform code that defines it. The generic
/// parameter exists purely to give platform-side code (which does hold a
/// compile-time reference) a type-safe way to query and register against this
/// extension point, without needing to cast at every call site.
/// </summary>
/// <typeparam name="TContract">The interface plugin implementations registered under this extension point must satisfy.</typeparam>
public sealed class ExtensionPointName<TContract> : IEquatable<ExtensionPointName<TContract>>
    where TContract : class
{
    /// <summary>
    /// The stable string identifier for this extension point, as referenced
    /// from plugin manifests (e.g. <c>"editor.completionContributor"</c>).
    /// Extension point IDs are conventionally dot-separated and namespaced by
    /// the subsystem that owns them, mirroring reverse-DNS style naming.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// A human-readable description of what this extension point is for,
    /// shown in developer-facing diagnostics and tooling (e.g. a "list all
    /// extension points" debug view).
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Creates a new extension point identifier.
    /// </summary>
    /// <param name="id">The stable string identifier for this extension point.</param>
    /// <param name="description">A human-readable description of the extension point's purpose.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="id"/> is null, empty, or whitespace.</exception>
    public ExtensionPointName(string id, string description)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Extension point id must not be null or empty.", nameof(id));
        }

        Id = id;
        Description = description ?? string.Empty;
    }

    /// <summary>
    /// Convenience factory, mirroring the common declaration pattern:
    /// <c>public static readonly ExtensionPointName&lt;ICompletionProvider&gt; COMPLETION_CONTRIBUTOR =
    /// ExtensionPointName&lt;ICompletionProvider&gt;.Create("editor.completionContributor", "...");</c>
    /// </summary>
    /// <param name="id">The stable string identifier for this extension point.</param>
    /// <param name="description">A human-readable description of the extension point's purpose.</param>
    /// <returns>A new extension point identifier.</returns>
    public static ExtensionPointName<TContract> Create(string id, string description)
        => new(id, description);

    /// <inheritdoc />
    public bool Equals(ExtensionPointName<TContract>? other)
    {
        if (other is null)
        {
            return false;
        }

        return ReferenceEquals(this, other) || string.Equals(Id, other.Id, StringComparison.Ordinal);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as ExtensionPointName<TContract>);

    /// <inheritdoc />
    public override int GetHashCode() => Id.GetHashCode(StringComparison.Ordinal);

    /// <inheritdoc />
    public override string ToString() => Id;

    /// <summary>Equality operator, delegating to <see cref="Equals(ExtensionPointName{TContract}?)"/>.</summary>
    public static bool operator ==(ExtensionPointName<TContract>? left, ExtensionPointName<TContract>? right)
        => left?.Equals(right) ?? right is null;

    /// <summary>Inequality operator, delegating to <see cref="Equals(ExtensionPointName{TContract}?)"/>.</summary>
    public static bool operator !=(ExtensionPointName<TContract>? left, ExtensionPointName<TContract>? right)
        => !(left == right);
}