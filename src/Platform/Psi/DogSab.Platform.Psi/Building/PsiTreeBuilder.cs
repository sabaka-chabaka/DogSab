using DogSab.Platform.Psi.Abstractions.Parsing;
using DogSab.Platform.Psi.Abstractions.Tree;
using DogSab.Platform.Psi.Tree;
using DogSab.Platform.Vfs.Abstractions.VirtualFile;

namespace DogSab.Platform.Psi.Building;

/// <summary>
/// Builds a complete <see cref="IPsiFile"/> for a virtual file: reads its
/// content, runs the language's <see cref="Abstractions.Lexing.ILexer"/> and
/// <see cref="Abstractions.Parsing.IParser"/> (obtained from its
/// <see cref="IParserDefinition"/>), and wraps the result as a
/// <see cref="PsiFileImpl"/>. The single place in the platform that
/// orchestrates lexing + parsing end to end for a file.
/// </summary>
public sealed class PsiTreeBuilder
{
    /// <summary>
    /// Builds a PSI file for the given virtual file using the given parser
    /// definition (already resolved by the caller via
    /// <see cref="Registry.LanguageRegistryImpl"/>, typically by file extension).
    /// </summary>
    /// <param name="file">The file to build a PSI tree for.</param>
    /// <param name="parserDefinition">The parser definition for the file's language.</param>
    /// <returns>The built PSI file.</returns>
    public IPsiFile Build(IVirtualFile file, IParserDefinition parserDefinition)
    {
        using var stream = file.OpenRead();
        using var reader = new System.IO.StreamReader(stream);
        var sourceText = reader.ReadToEnd();

        var lexer = parserDefinition.CreateLexer();
        var tokens = lexer.Tokenize(sourceText);

        var parser = parserDefinition.CreateParser();
        var rootElement = parser.Parse(tokens, sourceText);

        var fileRootType = rootElement.Type;
        var psiFile = new PsiFileImpl(fileRootType, sourceText.Length, sourceText, parserDefinition.Language, file);

        CopyChildren(rootElement, psiFile);

        return psiFile;
    }

    /// <summary>
    /// Copies the children the language's parser attached to its returned
    /// root element onto the platform's <see cref="PsiFileImpl"/> instance —
    /// necessary because <see cref="IParser.Parse"/> returns a plain
    /// <see cref="IPsiElement"/> (the language plugin's own tree root, which
    /// is not itself a <see cref="PsiFileImpl"/>), so its children must be
    /// re-parented onto the platform's actual file root rather than the
    /// parser's transient one.
    /// </summary>
    /// <param name="parsedRoot">The root element returned by the language's parser.</param>
    /// <param name="psiFile">The platform's actual file root to attach children onto.</param>
    private void CopyChildren(IPsiElement parsedRoot, PsiFileImpl psiFile)
    {
        foreach (var child in parsedRoot.Children)
        {
            if (child is PsiElementImpl childImpl)
            {
                psiFile.AddChild(childImpl);
            }
        }
    }
}