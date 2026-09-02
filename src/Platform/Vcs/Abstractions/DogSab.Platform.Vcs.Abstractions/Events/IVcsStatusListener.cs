using DogSab.Platform.Vfs.Abstractions.VirtualFile;

namespace DogSab.Platform.Vcs.Abstractions.Events;

/// <summary>
/// Listener interface for file VCS status changes, published on
/// <see cref="VcsExtensionPoints.VCS_STATUS_CHANGED"/>. Lets Project View
/// and other UI update a file's displayed status color without polling
/// <see cref="IVcsProvider.GetStatus"/> for every visible file on every render.
/// </summary>
public interface IVcsStatusListener
{
    /// <summary>
    /// Called when a file's version control status changes (e.g. after a
    /// commit, or after the file is edited).
    /// </summary>
    /// <param name="file">
    /// The file whose status changed.
    /// </param>
    /// <param name="newStatus">
    /// The file's new status.
    /// </param>
    void VcsStatusChanged(IVirtualFile file, FileVcsStatus newStatus);
}