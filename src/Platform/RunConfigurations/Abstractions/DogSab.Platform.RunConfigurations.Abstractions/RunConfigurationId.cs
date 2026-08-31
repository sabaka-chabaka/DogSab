namespace DogSab.Platform.RunConfigurations.Abstractions;

/// <summary>
/// A strongly-typed identifier for a single configured run configuration
/// (e.g. a user's saved "Run MyApp with --verbose" setup), wrapping a
/// stable <see cref="Guid"/>.
/// Backed by a <see cref="Guid"/> rather than a string — unlike
/// <c>ModuleId</c>/<c>IndexId</c>/<c>LanguageId</c> elsewhere in the
/// platform — because a run configuration is a user-created, per-instance
/// entity (the user can create any number of differently-named
/// configurations for the same project, e.g. "Debug" and "Release" run
/// configurations for the same module), not a single stable declaration
/// referenced by name from code or a manifest the way a module or language
/// is.
/// </summary>
public readonly struct RunConfigurationId : IEquatable<RunConfigurationId>
{
    /// <summary>
    /// The underlying identifier value.
    /// </summary>
    public Guid Value { get; }

    /// <summary>
    /// Creates a new run configuration identifier.
    /// </summary>
    /// <param name="value">
    /// The underlying GUID value.
    /// </param>
    public RunConfigurationId(Guid value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new, unique run configuration identifier.
    /// </summary>
    public static RunConfigurationId NewId() => new(Guid.NewGuid());

    /// <inheritdoc />
    public bool Equals(RunConfigurationId other) => Value.Equals(other.Value);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is RunConfigurationId other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => Value.GetHashCode();

    /// <inheritdoc />
    public override string ToString() => Value.ToString();

    /// <summary>
    /// Equality operator, delegating to <see cref="Equals(RunConfigurationId)"/>.
    /// </summary>
    public static bool operator ==(RunConfigurationId left, RunConfigurationId right) => left.Equals(right);

    /// <summary>
    /// Inequality operator, delegating to <see cref="Equals(RunConfigurationId)"/>.
    /// </summary>
    public static bool operator !=(RunConfigurationId left, RunConfigurationId right) => !left.Equals(right);
}