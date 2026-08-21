using DogSab.Platform.Psi.Abstractions.Language;
using DogSab.Platform.Psi.Abstractions.Tree;
using DogSab.Platform.Vfs.Abstractions.VirtualFile;

namespace DogSab.Platform.Psi.Tree;

/// <summary>
/// Default implementation of <see cref="IPsiFile"/>: the root of a file's PSI
/// tree. Structurally just a root node — inherits
/// <see cref="PsiElementImpl"/>'s child-management behavior — plus the
/// language it was parsed as and the virtual file it represents.
/// </summary>
public sealed class PsiFileImpl : PsiElementImpl, IPsiFile
{
    /// <inheritdoc />
    public ILanguage Language { get; }

    /// <inheritdoc />
    public IVirtualFile VirtualFile { get; }

    /// <summary>
    /// Creates a new PSI file root, passing <c>null</c> as its own containing
    /// file to the base constructor (a file can't reference itself before it
    /// exists), then overriding <see cref="ContainingFile"/> to correctly
    /// return <c>this</c> — polymorphically, regardless of whether callers
    /// hold this as <see cref="IPsiFile"/>, <see cref="IPsiElement"/>, or <see cref="PsiFileImpl"/>.
    /// </summary>
    /// <param name="type">The root node's type.</param>
    /// <param name="length">The full length of the file's text.</param>
    /// <param name="text">The file's full source text.</param>
    /// <param name="language">The language this file was parsed as.</param>
    /// <param name="virtualFile">The underlying virtual file this tree represents.</param>
    public PsiFileImpl(PsiElementType type, int length, string text, ILanguage language, IVirtualFile virtualFile)
        : base(type, startOffset: 0, length, text, containingFile: null)
    {
        Language = language;
        VirtualFile = virtualFile;
    }

    /// <inheritdoc />
    public override IPsiFile ContainingFile => this;
}