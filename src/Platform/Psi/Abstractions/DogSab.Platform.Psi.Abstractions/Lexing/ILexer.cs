namespace DogSab.Platform.Psi.Abstractions.Lexing;

/// <summary>
/// Breaks raw source text into a flat sequence of <see cref="IToken"/>
/// instances, the first stage of turning a file's text into a PSI tree.
/// Implemented per language by a language plugin (e.g. a C# lexer knows how
/// to recognize C# keywords, identifiers, and punctuation); the platform
/// itself has no built-in lexer.
/// </summary>
public interface ILexer
{
    /// <summary>
    /// Tokenizes the given source text.
    /// </summary>
    /// <param name="sourceText">The full text to tokenize.</param>
    /// <returns>The tokens found, in source order, covering the entire input without gaps or overlaps.</returns>
    IEnumerable<IToken> Tokenize(string sourceText);
}