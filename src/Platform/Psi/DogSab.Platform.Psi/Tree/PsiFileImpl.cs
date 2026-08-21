using DogSab.Platform.Psi.Abstractions.Language;
using DogSab.Platform.Psi.Abstractions.Tree;
using DogSab.Platform.Vfs.Abstractions.VirtualFile;

namespace DogSab.Platform.Psi.Tree;

/// <summary>
/// Default implementation of <see cref="IPsiFile"/>: the root of a file's PSI
/// tree. Inherits <see cref="PsiElementImpl"/>'s child-management behavior —
/// a file is structurally just a root node — and additionally carries the
/// language it was parsed as and the virtual file it represents.
/// </summary>
public sealed class PsiFileImpl : PsiElementImpl, IPsiFile
{
    /// <inheritdoc />
    public ILanguage Language { get; }

    /// <inheritdoc />
    public IVirtualFile VirtualFile { get; }

    /// <summary>
    /// Creates a new PSI file root. Note that this constructor cannot supply
    /// <c>containingFile</c> to the base <see cref="PsiElementImpl"/>
    /// constructor as itself, since the instance doesn't exist yet during
    /// base construction — <see cref="ContainingFileSelf"/> resolves this by
    /// overriding <see cref="IPsiElement.ContainingFile"/> to return <c>this</c> directly.
    /// </summary>
    /// <param name="type">The root node's type (typically a language-specific "file" element type).</param>
    /// <param name="length">The full length of the file's text.</param>
    /// <param name="text">The file's full source text.</param>
    /// <param name="language">The language this file was parsed as.</param>
    /// <param name="virtualFile">The underlying virtual file this tree represents.</param>
    public PsiFileImpl(PsiElementType type, int length, string text, ILanguage language, IVirtualFile virtualFile)
        : base(type, startOffset: 0, length, text, containingFile: null!)
    {
        Language = language;
        VirtualFile = virtualFile;
    }

    /// <summary>
    /// A file is its own containing file — overrides the base implementation
    /// (which would otherwise return the <c>null!</c> passed at construction,
    /// since a file can't reference itself before it exists) to correctly
    /// return <c>this</c>.
    /// </summary>
    public new IPsiFile ContainingFile => this;
}