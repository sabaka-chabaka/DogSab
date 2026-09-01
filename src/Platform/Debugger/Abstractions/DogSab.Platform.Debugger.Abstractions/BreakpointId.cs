namespace DogSab.Platform.Debugger.Abstractions;

/// <summary>
/// A strongly-typed identifier for a single set breakpoint, wrapping a
/// stable <see cref="Guid"/>.
/// Backed by a <see cref="Guid"/> for the same reason as
/// <c>RunConfigurationId</c> — a breakpoint is a user-created, per-instance
/// entity (the user can set any number of breakpoints across any files),
/// not a stable declaration referenced by name.
/// </summary>
public readonly struct BreakpointId : IEquatable<BreakpointId>
{
    /// <summary>
    /// The underlying identifier value.
    /// </summary>
    public Guid Value { get; }

    /// <summary>
    /// Creates a new breakpoint identifier.
    /// </summary>
    /// <param name="value">
    /// The underlying GUID value.
    /// </param>
    public BreakpointId(Guid value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new, unique breakpoint identifier.
    /// </summary>
    public static BreakpointId NewId() => new(Guid.NewGuid());

    /// <inheritdoc />
    public bool Equals(BreakpointId other) => Value.Equals(other.Value);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is BreakpointId other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => Value.GetHashCode();

    /// <inheritdoc />
    public override string ToString() => Value.ToString();

    /// <summary>
    /// Equality operator, delegating to <see cref="Equals(BreakpointId)"/>.
    /// </summary>
    public static bool operator ==(BreakpointId left, BreakpointId right) => left.Equals(right);

    /// <summary>
    /// Inequality operator, delegating to <see cref="Equals(BreakpointId)"/>.
    /// </summary>
    public static bool operator !=(BreakpointId left, BreakpointId right) => !left.Equals(right);
}