using DogSab.Platform.Vfs.Abstractions.VirtualFile;

namespace DogSab.Platform.Vfs.Abstractions.Documents;

/// <summary>
/// Contract for reconciling changes between the virtual file system and an
/// open in-memory document (the editor's live, possibly-unsaved buffer for a
/// file). Implemented by the future Editor module, not here — this
/// abstraction exists in Vfs.Abstractions because VFS is the side that
/// detects external changes and must decide what to do about a file that
/// also happens to be open in an editor with unsaved changes. Kept separate
/// from <see cref="Watching.IFileChangeListener"/> because the two have
/// different responsibilities: <c>IFileChangeListener</c> is a passive
/// notification ("this changed"), while this contract is asked to actively
/// resolve a potential conflict and report back what should happen.
/// </summary>
public interface IDocumentSyncListener
{
    /// <summary>
    /// Called when the underlying file for a currently open document has
    /// changed on disk (or in its backing store), outside of the editor
    /// itself. Implementations decide how to reconcile the external change
    /// with any unsaved in-memory edits — e.g. silently reload if there are
    /// no unsaved changes, or prompt the user to choose between keeping their
    /// edits and reloading the external version if there are.
    /// </summary>
    /// <param name="file">The file whose open document is affected.</param>
    /// <returns>
    /// The resolution the caller should apply to the open document, given the
    /// external change.
    /// </returns>
    DocumentSyncResolution OnExternalFileChanged(IVirtualFile file);
}

/// <summary>The outcome an <see cref="IDocumentSyncListener"/> decides for an externally changed file with an open document.</summary>
public enum DocumentSyncResolution
{
    /// <summary>Reload the document's content from the file system, discarding any unsaved in-memory edits.</summary>
    ReloadFromDisk,

    /// <summary>Keep the in-memory document as-is, ignoring the external change entirely.</summary>
    KeepInMemoryVersion,

    /// <summary>Defer the decision — typically because the listener needs to prompt the user asynchronously before resolving.</summary>
    Deferred
}