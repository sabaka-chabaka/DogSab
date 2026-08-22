using System.Collections.Concurrent;
using DogSab.Platform.Extensibility.Abstractions.ExtensionPoints;
using DogSab.Platform.Ui.Actions.Abstractions;

namespace DogSab.Platform.Ui.Actions;

/// <summary>
/// Resolves actions by ID for invocation from a menu click, toolbar button,
/// or keyboard shortcut. Actions themselves are discovered from the platform's
/// <see cref="IExtensionPointRegistry"/> (registered under
/// <see cref="ActionExtensionPoints.ACTION"/> by plugins), while this manager
/// additionally assigns each a stable string ID (since <see cref="AnAction"/>
/// itself carries no ID field — see the note in <see cref="RegisterActionId"/>)
/// and provides ID-based lookup for the keymap and menu-building code.
/// </summary>
public sealed class ActionManagerImpl
{
    private readonly IExtensionPointRegistry _extensionPointRegistry;
    private readonly ConcurrentDictionary<string, AnAction> _actionsById = new();

    public ActionManagerImpl(IExtensionPointRegistry extensionPointRegistry)
    {
        _extensionPointRegistry = extensionPointRegistry;
    }

    /// <summary>
    /// Associates a stable ID with an already-registered action, so it can
    /// later be looked up and bound to a keyboard shortcut via
    /// <see cref="Keymap.KeymapImpl"/>. Called once per action during startup
    /// registration (see <see cref="ActionRegistrationStartupActivity"/>),
    /// since <see cref="AnAction"/> itself has no ID property — actions are
    /// referenced by their manifest-declared extension registration, and the
    /// ID assignment happens at the point of registration rather than being
    /// baked into the action class itself, keeping <see cref="AnAction"/>
    /// focused purely on display/behavior.
    /// </summary>
    /// <param name="actionId">The stable ID to assign.</param>
    /// <param name="action">The action instance.</param>
    /// <exception cref="InvalidOperationException">Thrown if <paramref name="actionId"/> is already assigned to a different action.</exception>
    public void RegisterActionId(string actionId, AnAction action)
    {
        if (!_actionsById.TryAdd(actionId, action))
        {
            throw new InvalidOperationException($"Action id '{actionId}' is already registered.");
        }
    }

    /// <summary>
    /// Resolves an action by its assigned ID.
    /// </summary>
    /// <param name="actionId">The action's ID.</param>
    /// <returns>The action, or <c>null</c> if no action is registered under this ID.</returns>
    public AnAction? FindById(string actionId)
    {
        return _actionsById.TryGetValue(actionId, out var action) ? action : null;
    }

    /// <summary>Every action currently registered against <see cref="ActionExtensionPoints.ACTION"/>, regardless of whether it has an assigned ID yet.</summary>
    public IReadOnlyList<AnAction> AllActions => _extensionPointRegistry.GetExtensions(ActionExtensionPoints.ACTION);
}