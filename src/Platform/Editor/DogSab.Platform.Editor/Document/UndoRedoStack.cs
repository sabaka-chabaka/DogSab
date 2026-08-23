using DogSab.Platform.Editor.Abstractions.Document;

namespace DogSab.Platform.Editor.Document;

/// <summary>
/// Maintains the undo/redo history for a document as a sequence of applied
/// <see cref="DocumentChangeEvent"/> records. Undo replays a change's
/// inverse (swap <see cref="DocumentChangeEvent.OldText"/> and
/// <see cref="DocumentChangeEvent.NewText"/> back into the document); redo
/// replays the original change again. Pushing a new change while positioned
/// mid-history (after undoing) discards the abandoned redo branch — the
/// standard "linear undo" behavior most editors use, rather than a branching
/// undo tree.
/// </summary>
internal sealed class UndoRedoStack
{
    /// <summary>The full history of changes ever pushed, in chronological order.</summary>
    private readonly List<DocumentChangeEvent> _history = new();

    /// <summary>
    /// The index one past the last change currently "applied" — i.e. the
    /// next redo would apply <c>_history[_position]</c>, and the next undo
    /// would undo <c>_history[_position - 1]</c>. Equals <c>_history.Count</c>
    /// when fully caught up with no pending redos.
    /// </summary>
    private int _position;

    /// <summary>Whether an undo is currently available.</summary>
    public bool CanUndo => _position > 0;

    /// <summary>Whether a redo is currently available.</summary>
    public bool CanRedo => _position < _history.Count;

    /// <summary>
    /// Records a newly applied change. If the stack was positioned mid-history
    /// (some changes had been undone), those abandoned redo entries are discarded.
    /// </summary>
    /// <param name="change">The change that was just applied to the document.</param>
    public void Push(DocumentChangeEvent change)
    {
        if (_position < _history.Count)
        {
            _history.RemoveRange(_position, _history.Count - _position);
        }

        _history.Add(change);
        _position++;
    }

    /// <summary>
    /// Retrieves the change that should be undone next, moving the stack's
    /// position backward. Does not itself apply the inverse to any document —
    /// callers (<see cref="DocumentImpl"/>) are responsible for that.
    /// </summary>
    /// <returns>The change to undo, or <c>null</c> if <see cref="CanUndo"/> is <c>false</c>.</returns>
    public DocumentChangeEvent? PopForUndo()
    {
        if (!CanUndo)
        {
            return null;
        }

        _position--;
        return _history[_position];
    }

    /// <summary>
    /// Retrieves the change that should be redone next, moving the stack's
    /// position forward. Does not itself apply the change to any document.
    /// </summary>
    /// <returns>The change to redo, or <c>null</c> if <see cref="CanRedo"/> is <c>false</c>.</returns>
    public DocumentChangeEvent? PopForRedo()
    {
        if (!CanRedo)
        {
            return null;
        }

        var change = _history[_position];
        _position++;
        return change;
    }
}