using DogSab.Platform.ProjectModel.Abstractions.Roots;
using DogSab.Platform.Vfs.Abstractions.VirtualFile;

namespace DogSab.Platform.ProjectModel.Roots;

/// <summary>
/// Default, immutable implementation of <see cref="ISourceFolder"/>.
/// </summary>
public sealed class SourceFolderImpl : ISourceFolder
{
    /// <inheritdoc />
    public IVirtualFile Directory { get; }

    /// <inheritdoc />
    public SourceRootType Type { get; }

    /// <summary>
    /// Creates a new source folder entry.
    /// </summary>
    /// <param name="directory">The directory this source folder represents.</param>
    /// <param name="type">How this folder's contents should be treated by the platform.</param>
    public SourceFolderImpl(IVirtualFile directory, SourceRootType type)
    {
        Directory = directory;
        Type = type;
    }
}