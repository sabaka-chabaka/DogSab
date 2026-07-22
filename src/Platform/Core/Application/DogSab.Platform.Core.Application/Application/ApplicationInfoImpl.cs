using System.Reflection;
using DogSab.Platform.Core.Abstractions.Application;

namespace DogSab.Platform.Core.Application.Application;

/// <summary>
/// Default implementation of <see cref="IApplicationInfo"/>.
/// Reads version and build metadata from the entry assembly's attributes,
/// falling back to sensible defaults if any piece of metadata is missing
/// (e.g. during local development builds that were not stamped by CI).
/// </summary>
public sealed class ApplicationInfoImpl : IApplicationInfo
{
    /// <inheritdoc />
    public Version Version { get; }

    /// <inheritdoc />
    public string BuildNumber { get; }

    /// <inheritdoc />
    public bool IsEap { get; }

    /// <inheritdoc />
    public DateTime BuildDate { get; }

    /// <summary>
    /// Creates a new application info instance, reading metadata from the
    /// current process's entry assembly.
    /// </summary>
    public ApplicationInfoImpl()
    {
        var entryAssembly = Assembly.GetEntryAssembly();
        var assemblyName = entryAssembly?.GetName();

        Version = assemblyName?.Version ?? new Version(0, 0, 0);
        BuildNumber = ReadInformationalVersion(entryAssembly) ?? Version.ToString();
        IsEap = BuildNumber.Contains("eap", StringComparison.OrdinalIgnoreCase)
            || BuildNumber.Contains("preview", StringComparison.OrdinalIgnoreCase);
        BuildDate = ReadLinkerTimestamp(entryAssembly) ?? DateTime.UtcNow;
    }

    /// <summary>
    /// Reads the <see cref="AssemblyInformationalVersionAttribute"/> from the
    /// given assembly, if present, which typically carries a richer version
    /// string than the numeric <see cref="AssemblyName.Version"/> (e.g. including
    /// a git commit hash or pre-release suffix).
    /// </summary>
    /// <param name="assembly">The assembly to inspect, or <c>null</c>.</param>
    /// <returns>The informational version string, or <c>null</c> if unavailable.</returns>
    private static string? ReadInformationalVersion(Assembly? assembly)
    {
        return assembly?
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion;
    }

    /// <summary>
    /// Approximates the assembly's build date via its on-disk last-write timestamp,
    /// since .NET no longer embeds a reliable linker timestamp by default.
    /// </summary>
    /// <param name="assembly">The assembly to inspect, or <c>null</c>.</param>
    /// <returns>The approximate build date, or <c>null</c> if it could not be determined.</returns>
    private static DateTime? ReadLinkerTimestamp(Assembly? assembly)
    {
        if (assembly?.Location is not { Length: > 0 } location)
        {
            return null;
        }

        try
        {
            return System.IO.File.GetLastWriteTimeUtc(location);
        }
        catch (System.IO.IOException)
        {
            return null;
        }
    }
}