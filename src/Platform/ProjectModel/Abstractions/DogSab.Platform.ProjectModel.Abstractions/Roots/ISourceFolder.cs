using DogSab.Platform.Vfs.Abstractions.VirtualFile;

namespace DogSab.Platform.ProjectModel.Abstractions.Roots;

/// <summary>
/// A single classified subdirectory within a module's <see cref="IContentRoot"/>,
/// e.g. <c>src/</c> classified as <see cref="SourceRootType.Source"/> or
/// <c>tests/</c> classified as <see cref="SourceRootType.Test"/>.
/// </summary>
public interface ISourceFolder
{
    /// <summary>The directory this source folder represents.</summary>
    IVirtualFile Directory { get; }

    /// <summary>How this folder's contents should be treated by the platform.</summary>
    SourceRootType Type { get; }
}