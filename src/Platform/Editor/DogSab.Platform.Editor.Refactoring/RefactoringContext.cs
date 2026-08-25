using DogSab.Platform.Editor.Abstractions.Caret;
using DogSab.Platform.Editor.Abstractions.Document;
using DogSab.Platform.Psi.Abstractions.Tree;

namespace DogSab.Platform.Editor.Refactoring;

/// <summary>
/// The context a refactoring operates against: the file's parsed tree, the
/// live document it came from, and the position (typically the caret) the
/// refactoring was invoked at.
/// A refactoring uses <see cref="Position"/> to locate the specific
/// <see cref="IPsiElement"/> it should act on (e.g. the identifier under
/// the caret, for Rename) by walking the tree from the file root.
/// </summary>
public readonly struct RefactoringContext
{
    /// <summary>
    /// The file's parsed PSI tree.
    /// </summary>
    public IPsiFile PsiFile { get; }

    /// <summary>
    /// The live document the refactoring's changes will ultimately be
    /// applied to.
    /// </summary>
    public IDocument Document { get; }

    /// <summary>
    /// The position the refactoring was invoked at, typically the caret's
    /// current position.
    /// </summary>
    public TextPosition Position { get; }

    /// <summary>
    /// Creates a new refactoring context.
    /// </summary>
    /// <param name="psiFile">
    /// The file's parsed PSI tree.
    /// </param>
    /// <param name="document">
    /// The live document the refactoring will apply changes to.
    /// </param>
    /// <param name="position">
    /// The position the refactoring was invoked at.
    /// </param>
    public RefactoringContext(IPsiFile psiFile, IDocument document, TextPosition position)
    {
        PsiFile = psiFile;
        Document = document;
        Position = position;
    }
}