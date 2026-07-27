using System.Reflection;
using DogSab.Platform.Extensibility.Abstractions.Attributes;

namespace DogSab.Platform.Extensibility.Reflection;

/// <summary>
/// Scans an already-loaded assembly for classes marked with
/// <see cref="ExtensionAttribute"/>, as an alternative discovery path to
/// manifest-declared <see cref="Abstractions.Manifest.ExtensionDeclaration"/>
/// entries — primarily used for the platform's own internal extensions,
/// which ship inside a platform assembly rather than a separately loaded plugin.
/// </summary>
public sealed class ExtensionAttributeScanner
{
    /// <summary>
    /// Finds every public, non-abstract class in the given assembly carrying
    /// one or more <see cref="ExtensionAttribute"/> instances.
    /// </summary>
    /// <param name="assembly">The assembly to scan.</param>
    /// <returns>
    /// One entry per (class, extension point ID) pair found — a single class
    /// with multiple <see cref="ExtensionAttribute"/> instances yields multiple entries.
    /// </returns>
    public IReadOnlyList<ScannedExtension> Scan(Assembly assembly)
    {
        var results = new List<ScannedExtension>();

        foreach (var type in assembly.GetTypes())
        {
            if (!type.IsClass || type.IsAbstract || !type.IsPublic)
            {
                continue;
            }

            var extensionAttributes = type.GetCustomAttributes<ExtensionAttribute>(inherit: false);

            foreach (var attribute in extensionAttributes)
            {
                results.Add(new ScannedExtension(attribute.ExtensionPointId, type));
            }
        }

        return results;
    }
}

/// <summary>
/// A single class found by <see cref="ExtensionAttributeScanner.Scan"/>,
/// paired with the extension point ID it declared via <see cref="ExtensionAttribute"/>.
/// </summary>
public readonly struct ScannedExtension
{
    /// <summary>The extension point ID the class declared it implements.</summary>
    public string ExtensionPointId { get; }

    /// <summary>The class found carrying the <see cref="ExtensionAttribute"/>.</summary>
    public System.Type ImplementationType { get; }

    /// <summary>
    /// Creates a new scanned extension record.
    /// </summary>
    /// <param name="extensionPointId">The extension point ID the class declared.</param>
    /// <param name="implementationType">The class found carrying the attribute.</param>
    public ScannedExtension(string extensionPointId, System.Type implementationType)
    {
        ExtensionPointId = extensionPointId;
        ImplementationType = implementationType;
    }
}