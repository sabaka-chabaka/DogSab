using DogSab.Platform.Extensibility.Abstractions.Manifest;

namespace DogSab.Platform.Extensibility.Abstractions.Compatibility;

/// <summary>
/// Describes an inclusive/exclusive range of acceptable <see cref="PluginVersion"/>
/// values, as declared in a plugin manifest's dependency entries
/// (e.g. <c>"&gt;=1.0.0 &lt;2.0.0"</c>). Used to check whether an installed
/// plugin or platform version satisfies another plugin's declared requirement.
/// </summary>
public readonly struct VersionRange
{
    /// <summary>The minimum acceptable version, or <c>null</c> if there is no lower bound.</summary>
    public PluginVersion? MinVersion { get; }

    /// <summary>Whether <see cref="MinVersion"/> itself is included in the range.</summary>
    public bool MinInclusive { get; }

    /// <summary>The maximum acceptable version, or <c>null</c> if there is no upper bound.</summary>
    public PluginVersion? MaxVersion { get; }

    /// <summary>Whether <see cref="MaxVersion"/> itself is included in the range.</summary>
    public bool MaxInclusive { get; }

    /// <summary>
    /// Creates a new version range.
    /// </summary>
    /// <param name="minVersion">The minimum acceptable version, or <c>null</c> for no lower bound.</param>
    /// <param name="minInclusive">Whether <paramref name="minVersion"/> is included in the range.</param>
    /// <param name="maxVersion">The maximum acceptable version, or <c>null</c> for no upper bound.</param>
    /// <param name="maxInclusive">Whether <paramref name="maxVersion"/> is included in the range.</param>
    public VersionRange(PluginVersion? minVersion, bool minInclusive, PluginVersion? maxVersion, bool maxInclusive)
    {
        MinVersion = minVersion;
        MinInclusive = minInclusive;
        MaxVersion = maxVersion;
        MaxInclusive = maxInclusive;
    }

    /// <summary>
    /// Creates a range accepting any version greater than or equal to the given minimum, with no upper bound.
    /// </summary>
    /// <param name="minVersion">The minimum acceptable version, inclusive.</param>
    /// <returns>A new open-ended version range.</returns>
    public static VersionRange AtLeast(PluginVersion minVersion) => new(minVersion, true, null, false);

    /// <summary>
    /// Creates a range accepting only the exact given version.
    /// </summary>
    /// <param name="version">The exact required version.</param>
    /// <returns>A new single-version range.</returns>
    public static VersionRange Exact(PluginVersion version) => new(version, true, version, true);

    /// <summary>
    /// Creates a range accepting any version at all.
    /// </summary>
    /// <returns>A new fully unbounded version range.</returns>
    public static VersionRange Any() => new(null, false, null, false);

    /// <summary>
    /// Parses a version range expression combining zero or more comparator
    /// clauses separated by whitespace, e.g. <c>"&gt;=1.0.0 &lt;2.0.0"</c>,
    /// <c>"=1.5.2"</c>, or <c>"*"</c> for <see cref="Any"/>.
    /// </summary>
    /// <param name="expression">The version range expression to parse.</param>
    /// <returns>The parsed range.</returns>
    /// <exception cref="FormatException">Thrown if <paramref name="expression"/> is not a valid range expression.</exception>
    public static VersionRange Parse(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression) || expression.Trim() == "*")
        {
            return Any();
        }

        PluginVersion? min = null;
        var minInclusive = false;
        PluginVersion? max = null;
        var maxInclusive = false;

        foreach (var clause in expression.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            var (comparator, versionText) = SplitComparator(clause);
            var version = PluginVersion.Parse(versionText);

            switch (comparator)
            {
                case ">=":
                    min = version;
                    minInclusive = true;
                    break;
                case ">":
                    min = version;
                    minInclusive = false;
                    break;
                case "<=":
                    max = version;
                    maxInclusive = true;
                    break;
                case "<":
                    max = version;
                    maxInclusive = false;
                    break;
                case "=":
                    min = version;
                    minInclusive = true;
                    max = version;
                    maxInclusive = true;
                    break;
                default:
                    throw new FormatException($"Unrecognized version range comparator in clause '{clause}'.");
            }
        }

        return new VersionRange(min, minInclusive, max, maxInclusive);
    }

    /// <summary>
    /// Splits a single clause like <c>">=1.0.0"</c> into its comparator and version text.
    /// </summary>
    /// <param name="clause">The clause to split.</param>
    /// <returns>The comparator symbol and the remaining version text.</returns>
    /// <exception cref="FormatException">Thrown if the clause does not start with a recognized comparator.</exception>
    private static (string Comparator, string VersionText) SplitComparator(string clause)
    {
        foreach (var comparator in new[] { ">=", "<=", ">", "<", "=" })
        {
            if (clause.StartsWith(comparator, StringComparison.Ordinal))
            {
                return (comparator, clause[comparator.Length..]);
            }
        }

        throw new FormatException(
            $"Version range clause '{clause}' does not start with a recognized comparator (>=, <=, >, <, =).");
    }

    /// <summary>
    /// Checks whether the given version falls within this range.
    /// </summary>
    /// <param name="version">The version to check.</param>
    /// <returns><c>true</c> if <paramref name="version"/> satisfies this range; otherwise <c>false</c>.</returns>
    public bool Contains(PluginVersion version)
    {
        if (MinVersion is { } min)
        {
            var comparison = version.CompareTo(min);
            if (comparison < 0 || (comparison == 0 && !MinInclusive))
            {
                return false;
            }
        }

        if (MaxVersion is { } max)
        {
            var comparison = version.CompareTo(max);
            if (comparison > 0 || (comparison == 0 && !MaxInclusive))
            {
                return false;
            }
        }

        return true;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (MinVersion is null || MaxVersion is null)
        {
            return "*";
        }

        var parts = new List<string>();

        if (MinVersion is { } min)
        {
            parts.Add((MinInclusive ? ">=" : ">") + min);
        }

        if (MaxVersion is { } max)
        {
            parts.Add((MaxInclusive ? "<=" : "<") + max);
        }
        
        return string.Join(" ", parts);
    }
}