namespace DogSab.Platform.Psi.Abstractions.Language;

/// <summary>
/// Describes a single programming language known to the platform: its stable
/// identity, a display name, and the file extensions it claims. A language
/// plugin (e.g. <c>DogSab.Lang.CSharp</c>) implements this once and registers
/// it with <see cref="Registry.ILanguageRegistry"/>, alongside a matching
/// <see cref="Parsing.IParserDefinition"/> that supplies the actual
/// lexer/parser for it — <see cref="ILanguage"/> itself carries no parsing
/// logic, purely identity and metadata.
/// </summary>
public interface ILanguage
{
    /// <summary>The language's stable identifier.</summary>
    LanguageId Id { get; }

    /// <summary>A human-readable display name (e.g. <c>"C#"</c>), shown in the UI.</summary>
    string DisplayName { get; }

    /// <summary>
    /// File extensions (without the leading dot, e.g. <c>"cs"</c>) this
    /// language claims. Used by <see cref="Registry.ILanguageRegistry"/> to
    /// determine which language a file belongs to based on its name.
    /// </summary>
    IReadOnlyList<string> FileExtensions { get; }
}