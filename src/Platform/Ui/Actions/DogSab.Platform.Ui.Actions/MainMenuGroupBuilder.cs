using DogSab.Platform.Ui.Actions.Abstractions;

namespace DogSab.Platform.Ui.Actions;

/// <summary>
/// Assembles the platform's main menu bar structure (File, Edit, View, ...)
/// from every <see cref="AnAction"/> registered against
/// <see cref="ActionExtensionPoints.ACTION"/>, grouped by a
/// <see cref="MenuPlacementAttribute"/> declared on each action's type.
/// Actions without a placement attribute are omitted from the main menu
/// (they may still be reachable via keyboard shortcut or Search Everywhere,
/// once those exist) rather than causing a build failure — a plugin's action
/// not intended for the main menu shouldn't need to declare a placement at all.
/// </summary>
public sealed class MainMenuGroupBuilder
{
    private readonly ActionManagerImpl _actionManager;

    public MainMenuGroupBuilder(ActionManagerImpl actionManager)
    {
        _actionManager = actionManager;
    }

    /// <summary>
    /// Builds the root action group representing the main menu bar, with one
    /// top-level subgroup per distinct menu name found across all registered
    /// actions' placements.
    /// </summary>
    /// <returns>The assembled root group.</returns>
    public IActionGroup Build()
    {
        var byMenuName = new Dictionary<string, List<ActionGroupEntry>>();

        foreach (var action in _actionManager.AllActions)
        {
            var placement = action.GetType()
                .GetCustomAttributes(typeof(MenuPlacementAttribute), inherit: false)
                .Cast<MenuPlacementAttribute>()
                .FirstOrDefault();

            if (placement is null)
            {
                continue;
            }

            if (!byMenuName.TryGetValue(placement.MenuName, out var entries))
            {
                entries = new List<ActionGroupEntry>();
                byMenuName[placement.MenuName] = entries;
            }

            entries.Add(ActionGroupEntry.FromAction(action));
        }

        var topLevelGroups = byMenuName
            .Select(kvp => (IActionGroup)new SimpleActionGroup($"MainMenu.{kvp.Key}", kvp.Key, kvp.Value))
            .Select(ActionGroupEntry.FromSubGroup)
            .ToList();

        return new SimpleActionGroup("MainMenu", "Main Menu", topLevelGroups);
    }
}

/// <summary>Declares which top-level main menu an action belongs to (e.g. <c>"File"</c>, <c>"Edit"</c>).</summary>
[System.AttributeUsage(System.AttributeTargets.Class)]
public sealed class MenuPlacementAttribute : System.Attribute
{
    public string MenuName { get; }

    public MenuPlacementAttribute(string menuName)
    {
        MenuName = menuName;
    }
}

/// <summary>Minimal, immutable <see cref="IActionGroup"/> implementation used by <see cref="MainMenuGroupBuilder"/>.</summary>
internal sealed class SimpleActionGroup : IActionGroup
{
    public string Id { get; }
    public string DisplayText { get; }
    public System.Collections.Generic.IReadOnlyList<ActionGroupEntry> Children { get; }

    public SimpleActionGroup(string id, string displayText, System.Collections.Generic.IReadOnlyList<ActionGroupEntry> children)
    {
        Id = id;
        DisplayText = displayText;
        Children = children;
    }
}