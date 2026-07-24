namespace DogSab.Platform.Extensibility.Abstractions.Attributes;

/// <summary>
/// Marks the contract interface for an extension point, purely for
/// discoverability and documentation tooling — e.g. a future "list all
/// extension points" diagnostic view that scans loaded assemblies for
/// interfaces carrying this attribute to build a catalog of everything
/// plugins can extend, without needing a hand-maintained list. This attribute
/// is descriptive metadata only; it does not itself declare the extension
/// point with <see cref="ExtensionPoints.IExtensionPointRegistry"/> — that
/// still happens explicitly via
/// <see cref="ExtensionPoints.IExtensionPointRegistry.RegisterExtensionPoint{TContract}"/>,
/// typically alongside a static <see cref="ExtensionPoints.ExtensionPointName{TContract}"/>
/// field on the same or a related type.
/// </summary>
[AttributeUsage(AttributeTargets.Interface)]
public sealed class ExtensionPointDefinitionAttribute : Attribute
{
    /// <summary>
    /// The string identifier of the extension point this interface is the
    /// contract for, matching the corresponding
    /// <see cref="ExtensionPoints.ExtensionPointName{TContract}.Id"/>.
    /// </summary>
    public string ExtensionPointId { get; }

    /// <summary>
    /// Creates a new extension point definition marker.
    /// </summary>
    /// <param name="extensionPointId">The string identifier of the extension point this interface is the contract for.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="extensionPointId"/> is null, empty, or whitespace.</exception>
    public ExtensionPointDefinitionAttribute(string extensionPointId)
    {
        if (string.IsNullOrWhiteSpace(extensionPointId))
        {
            throw new ArgumentException("Extension point id must not be null or empty.", nameof(extensionPointId));
        }

        ExtensionPointId = extensionPointId;
    }
}