using DogSab.Platform.ProjectModel.Abstractions.Roots;
using DogSab.Platform.Vfs.Abstractions.VirtualFile;

namespace DogSab.Platform.ProjectModel.Roots;

/// <summary>
/// Default, immutable implementation of <see cref="IContentRoot"/>.
/// </summary>
public sealed class ContentRootImpl : IContentRoot
{
    /// <inheritdoc />
    public IVirtualFile RootDirectory { get; }

    /// <inheritdoc />
    public IReadOnlyList<ISourceFolder> SourceFolders { get; }

    /// <summary>
    /// Creates a new content root.
    /// </summary>
    /// <param name="rootDirectory">The root directory this content root is anchored at.</param>
    /// <param name="sourceFolders">The classified subfolders under this root.</param>
    public ContentRootImpl(IVirtualFile rootDirectory, IReadOnlyList<ISourceFolder> sourceFolders)
    {
        RootDirectory = rootDirectory;
        SourceFolders = sourceFolders;
    }
}