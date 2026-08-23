using Avalonia.Controls;
using DogSab.Platform.Ui.Actions;
using DogSab.Platform.Ui.Actions.Abstractions;

namespace DogSab.Platform.Ui.Shell;

/// <summary>
/// Builds an Avalonia <see cref="Menu"/> control from a platform
/// <see cref="IActionGroup"/> definition.
/// </summary>
public sealed class MenuBarBuilder
{
    private readonly ActionManagerImpl _actionManager;
    private readonly ActiveProjectTracker _activeProjectTracker;

    public MenuBarBuilder(ActionManagerImpl actionManager, ActiveProjectTracker activeProjectTracker)
    {
        _actionManager = actionManager;
        _activeProjectTracker = activeProjectTracker;
    }

    /// <summary>
    /// Builds a top-level menu bar from a root action group.
    /// </summary>
    /// <param name="rootGroup">The root group to build the menu bar from.</param>
    /// <returns>A ready-to-host Avalonia menu.</returns>
    public Menu Build(IActionGroup rootGroup)
    {
        var menu = new Menu();

        foreach (var entry in rootGroup.Children)
        {
            menu.Items.Add(BuildMenuItem(entry));
        }

        return menu;
    }

    private MenuItem BuildMenuItem(ActionGroupEntry entry)
    {
        return entry.Match(
            onAction: BuildActionMenuItem,
            onSubGroup: BuildSubGroupMenuItem);
    }

    private MenuItem BuildActionMenuItem(AnAction action)
    {
        var menuItem = new MenuItem { Header = action.DisplayText };

        menuItem.Click += (_, _) =>
        {
            var context = new ActionContext(_activeProjectTracker.ActiveProjectId);
            if (action.IsEnabled(context))
            {
                action.Execute(context);
            }
        };

        return menuItem;
    }

    private MenuItem BuildSubGroupMenuItem(IActionGroup subGroup)
    {
        var menuItem = new MenuItem { Header = subGroup.DisplayText };

        foreach (var childEntry in subGroup.Children)
        {
            menuItem.Items.Add(BuildMenuItem(childEntry));
        }

        return menuItem;
    }
}