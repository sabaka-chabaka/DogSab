namespace DogSab.Platform.Extensibility.Diagnostics;

/// <summary>
/// Thrown when a plugin-declared extension cannot be instantiated: the
/// implementation class named in the manifest could not be found in the
/// plugin's loaded assembly, does not implement the extension point's
/// declared contract, has no accessible parameterless constructor, or threw
/// during construction. Carries enough detail for the Plugin Manager UI to
/// explain exactly which extension declaration failed and why, rather than
/// surfacing a bare reflection exception to the user.
/// </summary>
public sealed class ExtensionInstantiationException : Exception
{
    /// <summary>The extension point ID the failing declaration was registered against.</summary>
    public string ExtensionPointId { get; }

    /// <summary>The fully-qualified class name that failed to instantiate, as declared in the plugin manifest.</summary>
    public string ImplementationClassName { get; }

    /// <summary>
    /// Creates a new exception describing a failed extension instantiation.
    /// </summary>
    /// <param name="extensionPointId">The extension point ID the declaration was registered against.</param>
    /// <param name="implementationClassName">The class name that failed to instantiate.</param>
    /// <param name="reason">A short, specific description of why instantiation failed (e.g. "type not found", "does not implement contract").</param>
    /// <param name="inner">The underlying exception, if the failure was caused by one (e.g. a constructor throwing).</param>
    public ExtensionInstantiationException(
        string extensionPointId,
        string implementationClassName,
        string reason,
        Exception? inner = null)
        : base(BuildMessage(extensionPointId, implementationClassName, reason), inner)
    {
        ExtensionPointId = extensionPointId;
        ImplementationClassName = implementationClassName;
    }

    /// <summary>
    /// Builds a message identifying both the failing extension point and class,
    /// so a failure buried in a long plugin-loading log is still traceable back
    /// to the exact manifest entry that caused it.
    /// </summary>
    /// <param name="extensionPointId">The extension point ID the declaration was registered against.</param>
    /// <param name="implementationClassName">The class name that failed to instantiate.</param>
    /// <param name="reason">A short description of why instantiation failed.</param>
    /// <returns>A descriptive message for the exception.</returns>
    private static string BuildMessage(string extensionPointId, string implementationClassName, string reason)
    {
        return $"Failed to instantiate extension '{implementationClassName}' for extension point " +
               $"'{extensionPointId}': {reason}";
    }
}