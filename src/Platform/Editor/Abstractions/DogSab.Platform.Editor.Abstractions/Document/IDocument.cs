namespace DogSab.Platform.Editor.Abstractions.Document;

/// <summary>
/// The live, in-memory, mutable text buffer for an open file — distinct from
/// <c>Vfs.Abstractions.VirtualFile.IVirtualFile.OpenRead()</c>, which gives a
/// point-in-time snapshot of the file's on-disk content. A document may
/// diverge from disk (unsaved edits) until explicitly saved. Maintains an
/// undo/redo history and a monotonically increasing version number, so other
/// subsystems (Psi's cache, Editor's completion) can cheaply check "has this
/// document changed since I last looked at it" via <see cref="Version"/>
/// rather than re-comparing full text content.
/// </summary>
public interface IDocument
{
    /// <summary>The document's full current text.</summary>
    string Text { get; }

    /// <summary>The document's current length, in characters.</summary>
    int Length { get; }

    /// <summary>
    /// A version number incremented on every change. Never decreases, even
    /// across undo — undoing a change still produces a new, higher version
    /// number representing "the text is now in this particular state",
    /// rather than rewinding to the version number the pre-undo state had.
    /// </summary>
    int Version { get; }

    /// <summary>
    /// Replaces a range of the document's text with new text, in a single atomic edit.
    /// </summary>
    /// <param name="offset">The character offset where the replaced range starts.</param>
    /// <param name="length">The length of the range to replace. Zero for a pure insertion.</param>
    /// <param name="newText">The text to insert in place of the replaced range. Empty for a pure deletion.</param>
    void Replace(int offset, int length, string newText);

    /// <summary>
    /// Undoes the most recent change, if any is available.
    /// </summary>
    /// <returns><c>true</c> if a change was undone; otherwise <c>false</c> (nothing to undo).</returns>
    bool Undo();

    /// <summary>
    /// Redoes the most recently undone change, if any is available.
    /// </summary>
    /// <returns><c>true</c> if a change was redone; otherwise <c>false</c> (nothing to redo).</returns>
    bool Redo();

    /// <summary>
    /// Subscribes a listener to this document's changes. Returns a
    /// disposable that unsubscribes when disposed, following the same
    /// connection-based unsubscribe pattern as <c>IMessageBusConnection</c>.
    /// </summary>
    /// <param name="listener">The listener to subscribe.</param>
    /// <returns>A disposable that unsubscribes the listener when disposed.</returns>
    System.IDisposable AddListener(IDocumentListener listener);
}