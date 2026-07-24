namespace DogSab.Platform.Extensibility.Diagnostics;

/// <summary>
/// Thrown when code attempts to declare an extension point whose string ID
/// has already been registered, whether with the same or a different contract
/// type. Extension point IDs must be globally unique across the platform and
/// all loaded plugins, since <see cref="ExtensionPoints.IExtensionPointRegistry"/>
/// resolves them purely by ID string when a plugin manifest references one.
/// </summary>
public sealed class DuplicateExtensionPointException : Exception
{
    /// <summary>The extension point ID that was already registered.</summary>
    public string ExtensionPointId { get; }

    /// <summary>The contract type of the extension point that was already registered.</summary>
    public Type ExistingContractType { get; }

    /// <summary>The contract type of the conflicting registration attempt.</summary>
    public Type AttemptedContractType { get; }

    /// <summary>
    /// Creates a new exception describing a duplicate extension point declaration.
    /// </summary>
    /// <param name="extensionPointId">The extension point ID that was already registered.</param>
    /// <param name="existingContractType">The contract type of the existing registration.</param>
    /// <param name="attemptedContractType">The contract type of the conflicting attempt.</param>
    public DuplicateExtensionPointException(string extensionPointId, Type existingContractType, Type attemptedContractType)
        : base(BuildMessage(extensionPointId, existingContractType, attemptedContractType))
    {
        ExtensionPointId = extensionPointId;
        ExistingContractType = existingContractType;
        AttemptedContractType = attemptedContractType;
    }

    /// <summary>
    /// Builds a message describing the conflict, noting whether it's a plain
    /// duplicate (same contract) or a genuine mismatch (different contracts),
    /// since the latter usually points to two unrelated subsystems accidentally
    /// choosing the same ID string.
    /// </summary>
    /// <param name="extensionPointId">The conflicting extension point ID.</param>
    /// <param name="existingContractType">The contract type already registered.</param>
    /// <param name="attemptedContractType">The contract type of the new attempt.</param>
    /// <returns>A descriptive message for the exception.</returns>
    private static string BuildMessage(string extensionPointId, Type existingContractType, Type attemptedContractType)
    {
        if (existingContractType == attemptedContractType)
        {
            return $"Extension point '{extensionPointId}' has already been registered with contract " +
                   $"'{existingContractType.FullName}'. Extension points can only be declared once.";
        }

        return $"Extension point '{extensionPointId}' was already registered with contract " +
               $"'{existingContractType.FullName}', but a new declaration attempted to use a different " +
               $"contract '{attemptedContractType.FullName}'. This likely means two unrelated subsystems " +
               $"accidentally chose the same extension point ID.";
    }
}