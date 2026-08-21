using DogSab.Platform.Psi.Abstractions.Parsing;
using DogSab.Platform.Psi.Abstractions.Tree;
using DogSab.Platform.Psi.Tree;
using DogSab.Platform.Vfs.Abstractions.VirtualFile;

namespace DogSab.Platform.Psi.Building;

/// <summary>
/// Builds a complete <see cref="IPsiFile"/> for a virtual file: reads its
/// content, runs the language's <see cref="Abstractions.Lexing.ILexer"/>,
/// then hands the platform's own <see cref="PsiFileImpl"/> root to the
/// language's <see cref="Abstractions.Parsing.IParser"/> via a
/// <see cref="PsiTreeWriterImpl"/>, so the resulting tree is always built
/// directly out of the platform's own node type — no separate tree to graft
/// on afterward, and no risk of a language plugin's own <see cref="IPsiElement"/>
/// implementation being silently dropped.
/// </summary>
public sealed class PsiTreeBuilder
{
    public IPsiFile Build(IVirtualFile file, IParserDefinition parserDefinition)
    {
        using var stream = file.OpenRead();
        using var reader = new System.IO.StreamReader(stream);
        var sourceText = reader.ReadToEnd();

        var lexer = parserDefinition.CreateLexer();
        var tokens = lexer.Tokenize(sourceText);

        var fileRootType = new Abstractions.Tree.PsiElementType($"{parserDefinition.Language.Id}.file");
        var psiFile = new PsiFileImpl(fileRootType, sourceText.Length, sourceText, parserDefinition.Language, file);

        var writer = new PsiTreeWriterImpl(psiFile, psiFile, sourceText);

        var parser = parserDefinition.CreateParser();
        parser.Parse(tokens, sourceText, writer);

        return psiFile;
    }
}