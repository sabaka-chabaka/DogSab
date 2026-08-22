using DogSab.Platform.Extensibility.Abstractions.ExtensionPoints;

namespace DogSab.Platform.Ui.Actions.Abstractions;

/// <summary>
/// Declares the platform's action extension point. Application-scoped —
/// actions are registered once per process by plugins, not per open project.
/// </summary>
public static class ActionExtensionPoints
{
    /// <summary>Contributes an invokable action to menus, toolbars, and keyboard shortcuts.</summary>
    public static readonly ExtensionPointName<AnAction> ACTION =
        ExtensionPointName<AnAction>.Create(
            "ui.action",
            "Contributes an invokable action to menus, toolbars, and keyboard shortcuts.");
}