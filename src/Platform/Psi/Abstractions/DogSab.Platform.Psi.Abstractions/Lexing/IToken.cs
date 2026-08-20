namespace DogSab.Platform.Psi.Abstractions.Lexing;

/// <summary>
/// A single lexical token produced by an <see cref="ILexer"/>: a classified
/// span of the source text (e.g. an identifier, a keyword, a punctuation
/// mark). Tokens are the flat, linear output of lexing, before
/// <see cref="Parsing.IParser"/> assembles them into a tree o
/// <see cref="Tree.IPsiElement"/> nodes.
/// </summary>
public interface IToken
{
    /// <summary>The kind of token this is. </summary>
    TokenType Type { get; }
    
    /// <summary>The zero-based character offset into the source text where this token starts.</summary>
    int StartOffset { get; }

    /// <summary>The length, in characters, of this token's span in the source text.</summary>
    int Length { get; }

    /// <summary>The token's exact text, as it appears in the source (e.g. the identifier's spelling).</summary>
    string Text { get; }
}