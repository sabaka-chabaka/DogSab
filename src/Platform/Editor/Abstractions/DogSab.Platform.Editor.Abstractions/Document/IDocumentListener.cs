namespace DogSab.Platform.Editor.Abstractions.Document;

/// <summary>
/// Listener interface for document content changes. Unlike most platform
/// listener interfaces, this is not published through a single shared
/// <c>ITopic</c> — each <see cref="IDocument"/> instance has its own
/// independent set of subscribers (see <see cref="IDocument.AddListener"/>),
/// since document changes are extremely high-frequency (every keystroke) and
/// listeners are almost always interested in one specific document (the one
/// currently being edited), not a global broadcast across every open document.
/// </summary>
public interface IDocumentListener
{
    /// <summary>Called after a change has been applied to the document.</summary>
    /// <param name="change">The change that was applied.</param>
    void DocumentChanged(DocumentChangeEvent change);
}