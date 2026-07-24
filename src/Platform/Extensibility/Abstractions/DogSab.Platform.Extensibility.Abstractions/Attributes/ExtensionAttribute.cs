namespace DogSab.Platform.Extensibility.Abstractions.Attributes;

/// <summary>
/// Marks a class as an implementation to be registered against a specific
/// extension point, as an alternative to declaring the registration in a
/// plugin's JSON manifest. Primarily intended for the platform's own internal
/// extensions — subsystems that ship inside the platform itself and can
/// therefore reference the extension point's string ID directly in code — and
/// for plugins that prefer attribute-based registration discovered via
/// assembly scanning over listing every extension explicitly in their manifest.
/// Manifest-declared extensions (see <see cref="Manifest.ExtensionDeclaration"/>)
/// remain the primary mechanism for third-party plugins, since they allow
/// dependency and compatibility checks to happen before any plugin code runs;
/// this attribute is a convenience for cases where that upfront manifest
/// listing would be pure duplication of what the code already states.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class ExtensionAttribute : Attribute
{
    /// <summary>
    /// The string identifier of the extension point this class should be
    /// registered against, matching some <see cref="ExtensionPoints.ExtensionPointName{TContract}.Id"/>.
    /// </summary>
    public string ExtensionPointId { get; }

    /// <summary>
    /// Creates a new extension marker.
    /// </summary>
    /// <param name="extensionPointId">The string identifier of the extension point this class implements.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="extensionPointId"/> is null, empty, or whitespace.</exception>
    public ExtensionAttribute(string extensionPointId)
    {
        if (string.IsNullOrWhiteSpace(extensionPointId))
        {
            throw new ArgumentException("Extension point id must not be null or empty.", nameof(extensionPointId));
        }

        ExtensionPointId = extensionPointId;
    }
}