using DogSab.Platform.Psi.Abstractions.Language;
using DogSab.Platform.Vfs.Abstractions.VirtualFile;

namespace DogSab.Platform.Psi.Abstractions.Tree;

/// <summary>
/// The root node of a file's PSI tree — an <see cref="IPsiElement"/> that
/// additionally knows which language it was parsed as and which
/// <see cref="IVirtualFile"/> it represents. One <see cref="IPsiFile"/>
/// exists per open/indexed file, cached and rebuilt on change by
/// <see cref="Caching.PsiFileCache"/> in the Impl assembly.
/// </summary>
public interface IPsiFile : IPsiElement
{
    /// <summary>The language this file was parsed as.</summary>
    ILanguage Language { get; }

    /// <summary>The underlying virtual file this PSI tree represents.</summary>
    IVirtualFile VirtualFile { get; }
}