using System.Collections.Concurrent;

namespace DogSab.Platform.Ui.Actions.Keymap;

/// <summary>
/// Maps action IDs to their bound keyboard shortcuts and back. A single
/// shortcut may be bound to at most one action at a time; binding a shortcut
/// already in use rebinds it, silently dropping the previous action's
/// binding — matching how most IDEs let a user freely reassign a shortcut,
/// with the "conflict" surfaced separately by a settings UI (not this class's
/// concern) rather than being an error here.
/// </summary>
public sealed class KeymapImpl
{
    private readonly ConcurrentDictionary<string, KeyboardShortcut> _shortcutByActionId = new();
    private readonly ConcurrentDictionary<KeyboardShortcut, string> _actionIdByShortcut = new();

    /// <summary>
    /// Binds a shortcut to an action, replacing any existing binding for
    /// either the action or the shortcut.
    /// </summary>
    /// <param name="actionId">The action's identifier.</param>
    /// <param name="shortcut">The shortcut to bind.</param>
    public void Bind(string actionId, KeyboardShortcut shortcut)
    {
        if (_shortcutByActionId.TryGetValue(actionId, out var previousShortcut))
        {
            _actionIdByShortcut.TryRemove(previousShortcut, out _);
        }

        if (_actionIdByShortcut.TryGetValue(shortcut, out var previousActionId))
        {
            _shortcutByActionId.TryRemove(previousActionId, out _);
        }

        _shortcutByActionId[actionId] = shortcut;
        _actionIdByShortcut[shortcut] = actionId;
    }

    /// <summary>
    /// Removes the shortcut binding for an action, if any.
    /// </summary>
    /// <param name="actionId">The action's identifier.</param>
    public void Unbind(string actionId)
    {
        if (_shortcutByActionId.TryRemove(actionId, out var shortcut))
        {
            _actionIdByShortcut.TryRemove(shortcut, out _);
        }
    }

    /// <summary>Looks up the shortcut bound to an action, if any.</summary>
    /// <param name="actionId">The action's identifier.</param>
    /// <returns>The bound shortcut, or <c>null</c> if unbound.</returns>
    public KeyboardShortcut? GetShortcutForAction(string actionId)
    {
        return _shortcutByActionId.TryGetValue(actionId, out var shortcut) ? shortcut : null;
    }

    /// <summary>Looks up which action a shortcut is bound to, if any.</summary>
    /// <param name="shortcut">The shortcut to look up.</param>
    /// <returns>The bound action's identifier, or <c>null</c> if unbound.</returns>
    public string? GetActionForShortcut(KeyboardShortcut shortcut)
    {
        return _actionIdByShortcut.TryGetValue(shortcut, out var actionId) ? actionId : null;
    }

    /// <summary>All currently bound (actionId, shortcut) pairs.</summary>
    public IReadOnlyDictionary<string, KeyboardShortcut> AllBindings => _shortcutByActionId;
}