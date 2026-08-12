using DogSab.Platform.Vfs.Abstractions.VirtualFile;

namespace DogSab.Platform.ProjectModel.Abstractions.Roots;

/// <summary>
/// The top-level directory a module's content lives under (typically the
/// module's own project directory). Contains one or more
/// <see cref="ISourceFolder"/> entries classifying its subdirectories. This
/// is the point where ProjectModel starts depending on Vfs — a content root
/// is identified by a real <see cref="IVirtualFile"/>, not a raw path string,
/// so it benefits from the same identity and change-notification guarantees
/// as any other file the platform works with.
/// </summary>
public interface IContentRoot
{
    /// <summary>The root directory this content root is anchored at.</summary>
    IVirtualFile RootDirectory { get; }

    /// <summary>The classified subfolders under this root.</summary>
    IReadOnlyList<ISourceFolder> SourceFolders { get; }
}