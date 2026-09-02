using DogSab.Platform.Vfs.Abstractions.VirtualFile;

namespace DogSab.Platform.Vcs.Abstractions;

/// <summary>
/// A single version control system integration (e.g. a future
/// <c>DogSab.Vcs.Git</c> plugin implementing this against LibGit2Sharp),
/// registered against <see cref="Events.VcsExtensionPoints.VCS_PROVIDER"/>.
/// The platform itself has no built-in notion of Git, or any other VCS —
/// it only knows the generic operations every VCS shares (status, commit,
/// history), delegated entirely to whichever provider claims a given
/// project directory.
/// </summary>
public interface IVcsProvider
{
    /// <summary>
    /// A stable identifier for this provider (e.g. <c>"git"</c>).
    /// </summary>
    string ProviderId { get; }

    /// <summary>
    /// A human-readable display name (e.g. <c>"Git"</c>), shown in VCS-related UI.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Determines whether this provider recognizes and can manage version
    /// control for the given directory — e.g. a Git provider checks for a
    /// <c>.git</c> directory somewhere up the directory's ancestry.
    /// </summary>
    /// <param name="directory">
    /// The directory to check.
    /// </param>
    /// <returns>
    /// <c>true</c> if this provider manages version control for this
    /// directory; otherwise <c>false</c>.
    /// </returns>
    bool OwnsDirectory(IVirtualFile directory);

    /// <summary>
    /// Retrieves the current version control status of a single file.
    /// </summary>
    /// <param name="file">
    /// The file to check.
    /// </param>
    /// <returns>
    /// The file's current status.
    /// </returns>
    FileVcsStatus GetStatus(IVirtualFile file);

    /// <summary>
    /// Commits the given files with the given message.
    /// </summary>
    /// <param name="files">
    /// The files to include in the commit. A provider is expected to stage
    /// these itself as part of the operation, rather than requiring a
    /// separate explicit staging step — matching how most IDE VCS
    /// integrations present commit as a single action over a chosen set
    /// of files.
    /// </param>
    /// <param name="message">
    /// The commit message.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>
    /// A task producing the newly created commit's information.
    /// </returns>
    Task<ICommitInfo> CommitAsync(IReadOnlyList<IVirtualFile> files, string message, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves the commit history affecting a single file, most recent first.
    /// </summary>
    /// <param name="file">
    /// The file to retrieve history for.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>
    /// A task producing the file's commit history.
    /// </returns>
    Task<IReadOnlyList<ICommitInfo>> GetHistoryAsync(IVirtualFile file, CancellationToken cancellationToken);
}