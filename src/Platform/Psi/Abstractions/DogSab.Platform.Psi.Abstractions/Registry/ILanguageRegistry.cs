using DogSab.Platform.Psi.Abstractions.Language;
using DogSab.Platform.Psi.Abstractions.Parsing;

namespace DogSab.Platform.Psi.Abstractions.Registry;

/// <summary>
/// The central registry through which language plugins declare the
/// languages they support, and through which the rest of the platform (PSI
/// building, Editor syntax highlighting, Indexing) resolves which
/// <see cref="IParserDefinition"/> applies to a given file. The same registry
/// pattern used throughout the platform (<c>ExtensionPointRegistryImpl</c>,
/// <c>VirtualFileSystemRegistry</c>, <c>IndexRegistry</c>) — registration by
/// stable ID, lookup by ID or, here, additionally by file extension.
/// </summary>
public interface ILanguageRegistry
{
    /// <summary>
    /// Registers a language plugin's parser definition, making its language
    /// discoverable by ID and by the file extensions it claims.
    /// </summary>
    /// <param name="parserDefinition">The parser definition to register.</param>
    /// <exception cref="System.InvalidOperationException">
    /// Thrown if a language is already registered under the same
    /// <see cref="ILanguage.Id"/>, or if any of its claimed file extensions
    /// are already claimed by a different registered language.
    /// </exception>
    void Register(IParserDefinition parserDefinition);

    /// <summary>
    /// Resolves the parser definition registered for a language by its ID.
    /// </summary>
    /// <param name="languageId">The language identifier to look up.</param>
    /// <returns>The registered parser definition, or <c>null</c> if no language is registered under this ID.</returns>
    IParserDefinition? FindByLanguageId(LanguageId languageId);

    /// <summary>
    /// Resolves the parser definition for whichever language claims a given
    /// file extension. Used to determine which language a file should be
    /// parsed as, based on its name.
    /// </summary>
    /// <param name="fileExtension">The file extension to look up (without the leading dot, e.g. <c>"cs"</c>).</param>
    /// <returns>The registered parser definition claiming this extension, or <c>null</c> if none does.</returns>
    IParserDefinition? FindByFileExtension(string fileExtension);

    /// <summary>Every currently registered language.</summary>
    IReadOnlyList<ILanguage> AllLanguages { get; }
}