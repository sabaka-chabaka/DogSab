using DogSab.Platform.Editor.Abstractions.Caret;
using DogSab.Platform.Editor.Abstractions.Document;
using DogSab.Platform.Editor.Abstractions.Selection;
using DogSab.Platform.Editor.Caret;
using DogSab.Platform.Editor.Document;
using DogSab.Platform.Editor.Selection;
using DogSab.Platform.Psi.Abstractions.Tree;
using DogSab.Platform.Vfs.Abstractions.VirtualFile;

namespace DogSab.Platform.Editor.Session;

/// <summary>
/// Holds together everything the platform needs to represent a single open
/// file being edited: its live <see cref="IDocument"/>, its
/// <see cref="ICaretModel"/>, its <see cref="ISelectionModel"/>, and the
/// <see cref="IVirtualFile"/> it was opened from.
/// One <see cref="EditorSession"/> exists per open editor tab, created when
/// a file is opened and disposed when its tab is closed. Distinct from
/// <see cref="Psi.Caching.PsiFileCache"/>'s cached <see cref="IPsiFile"/>
/// instances — a PSI file is a derived, read-mostly structural view rebuilt
/// on external change, while an <see cref="EditorSession"/>'s document is
/// the actual live, user-editable buffer that PSI is eventually reparsed
/// from once the user's edits settle.
/// </summary>
public sealed class EditorSession : IDisposable
{
    /// <summary>
    /// The virtual file this session was opened from, used to derive the
    /// document's initial content and, later, to save edits back to disk.
    /// </summary>
    public IVirtualFile VirtualFile { get; }

    /// <summary>
    /// The live, editable text buffer for this session.
    /// </summary>
    public IDocument Document { get; }

    /// <summary>
    /// The caret position(s) currently active within this session's document.
    /// </summary>
    public ICaretModel CaretModel { get; }

    /// <summary>
    /// The currently selected text range, if any, within this session's document.
    /// </summary>
    public ISelectionModel SelectionModel { get; }

    /// <summary>
    /// Whether this session's document has been disposed already, guarding
    /// against double-disposal.
    /// </summary>
    private bool _isDisposed;

    /// <summary>
    /// Creates a new editor session for a file, reading its current content
    /// from disk (via the virtual file) as the document's starting text.
    /// </summary>
    /// <param name="virtualFile">
    /// The file to open an editing session for.
    /// </param>
    public EditorSession(IVirtualFile virtualFile)
    {
        VirtualFile = virtualFile;

        var initialText = ReadInitialText(virtualFile);
        var documentImpl = new DocumentImpl(initialText);

        Document = documentImpl;
        CaretModel = new CaretModelImpl(documentImpl);
        SelectionModel = new SelectionModelImpl();
    }
    
    /// <summary>
    /// Reads a virtual file's current content as a string, used to seed a
    /// newly created document's initial text.
    /// </summary>
    /// <param name="virtualFile">
    /// The file to read.
    /// </param>
    /// <returns>
    /// The file's full text content.
    /// </returns>
    private static string ReadInitialText(IVirtualFile virtualFile)
    {
        using var stream = virtualFile.OpenRead();
        using var reader = new System.IO.StreamReader(stream);
        return reader.ReadToEnd();
    }
    
    /// <summary>
    /// Releases this session's resources.
    /// Currently a no-op beyond marking the session disposed, since none of
    /// <see cref="Document"/>, <see cref="CaretModel"/>, or
    /// <see cref="SelectionModel"/> hold unmanaged resources or external
    /// subscriptions of their own that require explicit teardown — kept as
    /// an explicit <see cref="IDisposable"/> regardless, so future state
    /// added to this session (e.g. a Psi file change subscription) has an
    /// obvious place to be cleaned up without changing this type's public shape.
    /// </summary>
    public void Dispose()
    {
        _isDisposed = true;
    }
}