using DogSab.Platform.Psi.Abstractions.Language; 
using DogSab.Platform.Psi.Abstractions.Lexing;

namespace DogSab.Platform.Psi.Abstractions.Parsing;

/// <summary>
/// Ties a single <see cref="ILanguage"/> to its matching <see cref="ILexer"/>
/// and <see cref="IParser"/> implementations. A language plugin registers
/// one of these with <see cref="Registry.ILanguageRegistry"/>, giving the
/// platform everything it needs to turn a file recognized as this language
/// into a PSI tree, without the platform ever needing to know the specific
/// lexing/parsing algorithm involved.
/// </summary>
public interface IParserDefinition
{
    /// <summary>The language this parser definition provides lexing/parsing for.</summary>
    ILanguage Language { get; }

    /// <summary>Creates a new lexer instance for this language.</summary>
    /// <returns>A new <see cref="ILexer"/> instance.</returns>
    ILexer CreateLexer();

    /// <summary>Creates a new parser instance for this language.</summary>
    /// <returns>A new <see cref="IParser"/> instance.</returns>
    IParser CreateParser();
}