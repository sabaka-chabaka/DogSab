using System.Collections.Concurrent;
using DogSab.Platform.Psi.Abstractions.Language;
using DogSab.Platform.Psi.Abstractions.Parsing;
using DogSab.Platform.Psi.Abstractions.Registry;

namespace DogSab.Platform.Psi.Registry;

/// <summary>
/// Default implementation of <see cref="ILanguageRegistry"/>. Maintains two
/// lookups over the same set of registrations: by <see cref="LanguageId"/>
/// (for direct lookups, e.g. when a manifest-driven feature needs a specific
/// language) and by file extension (for the common case of "which language
/// does this file belong to"), keeping both in sync on every registration.
/// </summary>
public sealed class LanguageRegistryImpl : ILanguageRegistry
{
    private readonly ConcurrentDictionary<LanguageId, IParserDefinition> _byLanguageId = new();
    private readonly ConcurrentDictionary<string, IParserDefinition> _byFileExtension = new(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public void Register(IParserDefinition parserDefinition)
    {
        var language = parserDefinition.Language;

        if (!_byLanguageId.TryAdd(language.Id, parserDefinition))
        {
            throw new InvalidOperationException($"A language is already registered under id '{language.Id}'.");
        }

        var claimedExtensions = new List<string>();

        try
        {
            foreach (var extension in language.FileExtensions)
            {
                if (!_byFileExtension.TryAdd(extension, parserDefinition))
                {
                    var conflictingLanguage = _byFileExtension[extension].Language;
                    throw new InvalidOperationException(
                        $"File extension '{extension}' is already claimed by language '{conflictingLanguage.Id}'; " +
                        $"cannot register it for language '{language.Id}'.");
                }

                claimedExtensions.Add(extension);
            }
        }
        catch
        {
            // Roll back partial registration: if extension N of M conflicts,
            // undo both the language-id registration and every extension
            // successfully claimed before the conflict, so a failed
            // Register call never leaves the registry in an inconsistent,
            // half-registered state.
            _byLanguageId.TryRemove(language.Id, out _);
            foreach (var extension in claimedExtensions)
            {
                _byFileExtension.TryRemove(extension, out _);
            }

            throw;
        }
    }

    /// <inheritdoc />
    public IParserDefinition? FindByLanguageId(LanguageId languageId)
    {
        return _byLanguageId.TryGetValue(languageId, out var definition) ? definition : null;
    }

    /// <inheritdoc />
    public IParserDefinition? FindByFileExtension(string fileExtension)
    {
        return _byFileExtension.TryGetValue(fileExtension, out var definition) ? definition : null;
    }

    /// <inheritdoc />
    public IReadOnlyList<ILanguage> AllLanguages => _byLanguageId.Values.Select(d => d.Language).ToList();
}