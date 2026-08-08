namespace DogSab.Platform.ProjectModel.Abstractions.Project;

/// <summary>
/// A strongly-typed identifier for a project within a solution. Distinct from
/// <c>Core.Application.ProjectLifecycle.ProjectSession.ProjectId</c> — that
/// one identifies an open workspace session (there is one per opened
/// project, generated fresh each time it's opened), while this one
/// identifies a project as a structural node within the <see cref="Solution.ISolution"/>
/// model itself, and can be persisted across sessions.
/// </summary>
public readonly struct ProjectId : IEquatable<ProjectId>
{
    /// <summary>The underlying identifier value.</summary>
    public Guid Value { get; }

    /// <summary>
    /// Creates a new project identifier.
    /// </summary>
    /// <param name="value">The underlying GUID value.</param>
    public ProjectId(Guid value)
    {
        Value = value;
    }
    
    /// <summary>Creates a new, unique project identifier.</summary>
    public static ProjectId NewId() => new(Guid.NewGuid());

    /// <inheritdoc />
    public bool Equals(ProjectId other) => Value.Equals(other.Value);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is ProjectId other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => Value.GetHashCode();

    /// <inheritdoc />
    public override string ToString() => Value.ToString();
    
    /// <summary>Equality operator, delegating to <see cref="Equals(ProjectId)"/>.</summary>
    public static bool operator ==(ProjectId left, ProjectId right) => left.Equals(right);

    /// <summary>Inequality operator, delegating to <see cref="Equals(ProjectId)"/>.</summary>
    public static bool operator !=(ProjectId left, ProjectId right) => !left.Equals(right);
}