namespace DogSab.Platform.Extensibility.Abstractions.Manifest;

/// <summary>
/// A single extension registration declared in a plugin's manifest: which
/// extension point it registers against, and the fully-qualified name of the
/// class implementing that extension point's contract. Resolved and
/// instantiated by the plugin loader once the plugin's assembly has been loaded.
/// </summary>
public readonly struct ExtensionDeclaration
{
    /// <summary>
    /// The string identifier of the extension point being registered against,
    /// matching some <see cref="ExtensionPoints.ExtensionPointName{TContract}.Id"/>
    /// declared by the platform or another plugin.
    /// </summary>
    public string ExtensionPointId { get; }

    /// <summary>
    /// The fully-qualified name (namespace and type) of the class implementing
    /// the extension point's contract, to be located via reflection within the
    /// plugin's loaded assembly.
    /// </summary>
    public string ImplementationClassName { get; }

    /// <summary>
    /// Creates a new extension declaration.
    /// </summary>
    /// <param name="extensionPointId">The string identifier of the extension point being registered against.</param>
    /// <param name="implementationClassName">The fully-qualified name of the implementing class.</param>
    public ExtensionDeclaration(string extensionPointId, string implementationClassName)
    {
        ExtensionPointId = extensionPointId;
        ImplementationClassName = implementationClassName;
    }

    /// <inheritdoc />
    public override string ToString() => $"{ExtensionPointId} -> {ImplementationClassName}";
}