namespace DogSab.Platform.ProjectModel.Abstractions.Module;

/// <summary>
/// A strongly-typed identifier for a module within a project. Unlike
/// <see cref="Solution.SolutionId"/> and <see cref="Project.ProjectId"/>,
/// backed by a string rather than a <see cref="Guid"/>, since modules are
/// conventionally referred to by a stable, human-meaningful name (e.g.
/// <c>"MyApp.Core"</c>) both in <see cref="ModuleDependency"/> declarations
/// and when persisted to disk — a random GUID would make persisted project
/// files unreadable and dependency declarations unreviewable in a diff.
/// </summary>
public readonly struct ModuleId : IEquatable<ModuleId>
{
    /// <summary>The module's stable name.</summary>
    public string Value { get; }

    /// <summary>
    /// Creates a new module identifier.
    /// </summary>
    /// <param name="value">The module's stable name.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is null, empty, or whitespace.</exception>
    public ModuleId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Module id must not be null or empty.", nameof(value));
        }

        Value = value;
    }

    /// <inheritdoc />
    public bool Equals(ModuleId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is ModuleId other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    /// <inheritdoc />
    public override string ToString() => Value;

    /// <summary>Equality operator, delegating to <see cref="Equals(ModuleId)"/>.</summary>
    public static bool operator ==(ModuleId left, ModuleId right) => left.Equals(right);

    /// <summary>Inequality operator, delegating to <see cref="Equals(ModuleId)"/>.</summary>
    public static bool operator !=(ModuleId left, ModuleId right) => !left.Equals(right);
}