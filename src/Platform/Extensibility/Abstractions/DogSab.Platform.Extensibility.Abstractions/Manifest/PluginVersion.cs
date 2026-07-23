namespace DogSab.Platform.Extensibility.Abstractions.Manifest;

/// <summary>
/// A semantic version for a plugin: <c>Major.Minor.Patch</c> with an optional
/// pre-release suffix (e.g. <c>1.2.0-beta.1</c>). Comparison follows semver
/// precedence rules: numeric fields compare numerically, and a version with a
/// pre-release suffix is considered lower than the same version without one.
/// </summary>
public readonly struct PluginVersion : IEquatable<PluginVersion>, IComparable<PluginVersion>
{
    /// <summary>The major version component.</summary>
    public int Major { get; }

    /// <summary>The minor version component.</summary>
    public int Minor { get; }

    /// <summary>The patch version component.</summary>
    public int Patch { get; }

    /// <summary>The pre-release suffix (e.g. <c>"beta.1"</c>), or <c>null</c> if this is a release version.</summary>
    public string? PreRelease { get; }

    /// <summary>
    /// Creates a new plugin version.
    /// </summary>
    /// <param name="major">The major version component.</param>
    /// <param name="minor">The minor version component.</param>
    /// <param name="patch">The patch version component.</param>
    /// <param name="preRelease">An optional pre-release suffix.</param>
    public PluginVersion(int major, int minor, int patch, string? preRelease = null)
    {
        Major = major;
        Minor = minor;
        Patch = patch;
        PreRelease = string.IsNullOrWhiteSpace(preRelease) ? null : preRelease;
    }

    /// <summary>
    /// Parses a version string of the form <c>Major.Minor.Patch</c> or
    /// <c>Major.Minor.Patch-PreRelease</c>.
    /// </summary>
    /// <param name="value">The version string to parse.</param>
    /// <returns>The parsed version.</returns>
    /// <exception cref="FormatException">Thrown if <paramref name="value"/> is not a valid version string.</exception>
    public static PluginVersion Parse(string value)
    {
        if (!TryParse(value, out var result))
        {
            throw new FormatException($"'{value}' is not a valid plugin version string. Expected format: Major.Minor.Patch[-PreRelease].");
        }

        return result;
    }

    /// <summary>
    /// Attempts to parse a version string of the form <c>Major.Minor.Patch</c>
    /// or <c>Major.Minor.Patch-PreRelease</c>.
    /// </summary>
    /// <param name="value">The version string to parse.</param>
    /// <param name="result">The parsed version, if parsing succeeded.</param>
    /// <returns><c>true</c> if parsing succeeded; otherwise <c>false</c>.</returns>
    public static bool TryParse(string? value, out PluginVersion result)
    {
        result = default;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var corePart = value;
        string? preRelease = null;

        var dashIndex = value.IndexOf('-');
        if (dashIndex >= 0)
        {
            corePart = value[..dashIndex];
            preRelease = value[(dashIndex + 1)..];
        }

        var segments = corePart.Split('.');
        if (segments.Length != 3)
        {
            return false;
        }

        if (!int.TryParse(segments[0], out var major) ||
            !int.TryParse(segments[1], out var minor) ||
            !int.TryParse(segments[2], out var patch))
        {
            return false;
        }

        if (major < 0 || minor < 0 || patch < 0)
        {
            return false;
        }

        result = new PluginVersion(major, minor, patch, preRelease);
        return true;
    }

    /// <inheritdoc />
    public bool Equals(PluginVersion other) =>
        Major == other.Major && Minor == other.Minor && Patch == other.Patch &&
        string.Equals(PreRelease, other.PreRelease, StringComparison.Ordinal);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is PluginVersion other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Major, Minor, Patch, PreRelease);

    /// <summary>
    /// Compares this version to another following semver precedence: numeric
    /// fields compare numerically first; if all are equal, a version without a
    /// pre-release suffix is considered greater than one with a suffix, and two
    /// pre-release suffixes are compared ordinally.
    /// </summary>
    /// <param name="other">The version to compare against.</param>
    /// <returns>A negative value if this version is lower, zero if equal, a positive value if higher.</returns>
    public int CompareTo(PluginVersion other)
    {
        var majorComparison = Major.CompareTo(other.Major);
        if (majorComparison != 0) return majorComparison;

        var minorComparison = Minor.CompareTo(other.Minor);
        if (minorComparison != 0) return minorComparison;

        var patchComparison = Patch.CompareTo(other.Patch);
        if (patchComparison != 0) return patchComparison;

        return ComparePreRelease(PreRelease, other.PreRelease);
    }

    /// <summary>
    /// Compares two pre-release suffixes per semver rules: no suffix (<c>null</c>)
    /// outranks any suffix; two suffixes compare ordinally.
    /// </summary>
    /// <param name="left">The first pre-release suffix, or <c>null</c>.</param>
    /// <param name="right">The second pre-release suffix, or <c>null</c>.</param>
    /// <returns>A negative value if <paramref name="left"/> is lower, zero if equal, a positive value if higher.</returns>
    private static int ComparePreRelease(string? left, string? right)
    {
        if (left is null && right is null) return 0;
        if (left is null) return 1;  // no pre-release outranks having one
        if (right is null) return -1;
        return string.CompareOrdinal(left, right);
    }

    /// <inheritdoc />
    public override string ToString() =>
        PreRelease is null ? $"{Major}.{Minor}.{Patch}" : $"{Major}.{Minor}.{Patch}-{PreRelease}";

    /// <summary>Equality operator, delegating to <see cref="Equals(PluginVersion)"/>.</summary>
    public static bool operator ==(PluginVersion left, PluginVersion right) => left.Equals(right);

    /// <summary>Inequality operator, delegating to <see cref="Equals(PluginVersion)"/>.</summary>
    public static bool operator !=(PluginVersion left, PluginVersion right) => !left.Equals(right);

    /// <summary>Less-than operator, per semver precedence.</summary>
    public static bool operator <(PluginVersion left, PluginVersion right) => left.CompareTo(right) < 0;

    /// <summary>Greater-than operator, per semver precedence.</summary>
    public static bool operator >(PluginVersion left, PluginVersion right) => left.CompareTo(right) > 0;

    /// <summary>Less-than-or-equal operator, per semver precedence.</summary>
    public static bool operator <=(PluginVersion left, PluginVersion right) => left.CompareTo(right) <= 0;

    /// <summary>Greater-than-or-equal operator, per semver precedence.</summary>
    public static bool operator >=(PluginVersion left, PluginVersion right) => left.CompareTo(right) >= 0;
}