namespace DogSab.Platform.Vfs.Abstractions.Watching;

/// <summary>
/// Listener interface for file change notifications, published on
/// <see cref="VfsTopics.FILE_CHANGED_UI"/> and <see cref="VfsTopics.FILE_CHANGED_BACKGROUND"/>.
/// Subscribers pick whichever topic matches their delivery needs — see the
/// remarks on <see cref="VfsTopics"/> for why there are two.
/// </summary>
public interface IFileChangeListener
{
    /// <summary>Called when a file change is published on a subscribed topic.</summary>
    /// <param name="args">Details of the change that occurred.</param>
    void OnFileChanged(FileChangeEvent args);
}