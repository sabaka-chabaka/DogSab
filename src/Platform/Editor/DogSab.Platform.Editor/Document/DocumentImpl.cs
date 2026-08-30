using System.Text;
using DogSab.Platform.Editor.Abstractions.Caret;
using DogSab.Platform.Editor.Abstractions.Document;

namespace DogSab.Platform.Editor.Document;

/// <summary>
/// Default implementation of <see cref="IDocument"/>.
/// Holds the document's full text in a <see cref="StringBuilder"/> for
/// efficient in-place mutation, delegates line/column tracking to
/// <see cref="LineIndex"/>, and delegates undo/redo bookkeeping to
/// <see cref="UndoRedoStack"/>.
/// </summary>
public sealed class DocumentImpl : IDocument
{
    /// <summary>
    /// The document's text, stored as a mutable buffer rather than an
    /// immutable string, so that <see cref="Replace"/> can splice a range
    /// without allocating a full copy of the entire document on every edit.
    /// </summary>
    private readonly StringBuilder _text;

    /// <summary>
    /// Tracks line-start offsets incrementally, so callers can convert
    /// between flat offsets and (line, column) positions without an O(n)
    /// rescan on every lookup.
    /// </summary>
    private readonly LineIndex _lineIndex = new();

    /// <summary>
    /// Records the change history so edits can be undone and redone.
    /// </summary>
    private readonly UndoRedoStack _undoRedoStack = new();

    /// <summary>
    /// The listeners currently subscribed to this document's changes.
    /// A plain list rather than the platform's shared message bus, since
    /// document listeners are almost always scoped to a single specific
    /// document instance rather than a global broadcast — the same
    /// reasoning already documented on <see cref="IDocumentListener"/>.
    /// </summary>
    private readonly List<IDocumentListener> _listeners = new();

    /// <inheritdoc />
    public string Text => _text.ToString();

    /// <inheritdoc />
    public int Length => _text.Length;

    /// <inheritdoc />
    public int Version { get; private set; }

    /// <summary>
    /// Creates a new document with the given initial content.
    /// </summary>
    /// <param name="initialText">
    /// The document's starting text, typically read from the backing
    /// <see cref="Vfs.Abstractions.VirtualFile.IVirtualFile"/> when a file
    /// is first opened for editing.
    /// </param>
    public DocumentImpl(string initialText)
    {
        _text = new StringBuilder(initialText);
        _lineIndex.Rebuild(initialText);
    }

    /// <inheritdoc />
    public void Replace(int offset, int length, string newText)
    {
        var oldText = _text.ToString(offset, length);

        _text.Remove(offset, length);
        _text.Insert(offset, newText);

        Version++;

        _lineIndex.ApplyChange(offset, length, newText, _text.ToString());

        var change = new DocumentChangeEvent(offset, oldText, newText, Version);
        _undoRedoStack.Push(change);

        NotifyListeners(change);
    }

    /// <inheritdoc />
    public bool Undo()
    {
        var changeToUndo = _undoRedoStack.PopForUndo();

        if (changeToUndo is not { } change)
        {
            return false;
        }

        // Undoing a change means applying its inverse: replace the range
        // that currently holds NewText back with OldText. This is applied
        // directly to the buffer/index rather than going through Replace(),
        // since Replace() would itself push a new entry onto the undo
        // stack — undo must not create a new undoable action, only move
        // the stack's position backward, which PopForUndo already did.
        ApplyRawChange(change.Offset, change.NewText.Length, change.OldText);

        var inverseEvent = new DocumentChangeEvent(change.Offset, change.NewText, change.OldText, Version);
        NotifyListeners(inverseEvent);

        return true;
    }
    
    /// <inheritdoc />
    public TextPosition ResolvePosition(int offset)
    {
        return _lineIndex.ToPosition(offset);
    }

    /// <inheritdoc />
    public int ResolveOffset(int line, int column)
    {
        return _lineIndex.ToOffset(line, column);
    }

    /// <inheritdoc />
    public bool Redo()
    {
        var changeToRedo = _undoRedoStack.PopForRedo();

        if (changeToRedo is not { } change)
        {
            return false;
        }

        ApplyRawChange(change.Offset, change.OldText.Length, change.NewText);

        var redoEvent = new DocumentChangeEvent(change.Offset, change.OldText, change.NewText, Version);
        NotifyListeners(redoEvent);

        return true;
    }

    /// <inheritdoc />
    public IDisposable AddListener(IDocumentListener listener)
    {
        _listeners.Add(listener);
        return new ListenerRegistration(this, listener);
    }

    /// <summary>
    /// Applies a text replacement directly to the buffer and line index,
    /// bumping the version, without touching the undo/redo stack.
    /// Used by <see cref="Undo"/> and <see cref="Redo"/>, which manage the
    /// stack's position themselves and must not have this helper push a
    /// competing entry onto it.
    /// </summary>
    /// <param name="offset">
    /// The character offset where the replaced range starts.
    /// </param>
    /// <param name="length">
    /// The length of the range being replaced.
    /// </param>
    /// <param name="replacementText">
    /// The text to insert in place of the replaced range.
    /// </param>
    private void ApplyRawChange(int offset, int length, string replacementText)
    {
        _text.Remove(offset, length);
        _text.Insert(offset, replacementText);

        Version++;

        _lineIndex.ApplyChange(offset, length, replacementText, _text.ToString());
    }

    /// <summary>
    /// Notifies every currently subscribed listener of a change, in
    /// subscription order.
    /// </summary>
    /// <param name="change">
    /// The change to report to listeners.
    /// </param>
    private void NotifyListeners(DocumentChangeEvent change)
    {
        foreach (var listener in _listeners)
        {
            listener.DocumentChanged(change);
        }
    }

    /// <summary>
    /// Disposable handle returned from <see cref="AddListener"/>.
    /// Removing the listener from the owning document's list on disposal,
    /// following the same connection-based unsubscribe pattern already used
    /// throughout the platform (e.g. <c>IMessageBusConnection</c>).
    /// </summary>
    private sealed class ListenerRegistration : IDisposable
    {
        private readonly DocumentImpl _owner;
        private readonly IDocumentListener _listener;
        private bool _isDisposed;

        /// <summary>
        /// Creates a new listener registration handle.
        /// </summary>
        /// <param name="owner">
        /// The document this listener was subscribed to.
        /// </param>
        /// <param name="listener">
        /// The listener instance to remove on disposal.
        /// </param>
        public ListenerRegistration(DocumentImpl owner, IDocumentListener listener)
        {
            _owner = owner;
            _listener = listener;
        }

        /// <summary>
        /// Removes the associated listener from the owning document.
        /// Safe to call multiple times; subsequent calls are no-ops.
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            _owner._listeners.Remove(_listener);
        }
    }
}