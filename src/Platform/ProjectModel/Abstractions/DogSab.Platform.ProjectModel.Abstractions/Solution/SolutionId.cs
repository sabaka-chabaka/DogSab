namespace DogSab.Platform.ProjectModel.Abstractions.Solution;

/// <summary>
/// A strongly-typed identifier for a solution, wrapping a stable value so
/// solution IDs cannot be accidentally interchanged with project or module
/// IDs at call sites, the same rationale as <c>PluginId</c> in Extensibility.Abstractions.
/// </summary>
public readonly struct SolutionId : IEquatable<SolutionId>
{
    /// <summary>The underlying identifier value.</summary>
    public Guid Value { get; }

    /// <summary>
    /// Creates a new solution identifier.
    /// </summary>
    /// <param name="value">The underlying GUID value.</param>
    public SolutionId(Guid value)
    {
        Value = value;
    }
    
    /// <summary>Creates a new, unique solution identifier.</summary>
    public static SolutionId NewId() => new(Guid.NewGuid());

    /// <inheritdoc />
    public bool Equals(SolutionId other) => Value.Equals(other.Value);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is SolutionId other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => Value.GetHashCode();

    /// <inheritdoc />
    public override string ToString() => Value.ToString();

    /// <summary>Equality operator, delegating to <see cref="Equals(SolutionId)"/>.</summary>
    public static bool operator ==(SolutionId left, SolutionId right) => left.Equals(right);

    /// <summary>Inequality operator, delegating to <see cref="Equals(SolutionId)"/>.</summary>
    public static bool operator !=(SolutionId left, SolutionId right) => !left.Equals(right);

}