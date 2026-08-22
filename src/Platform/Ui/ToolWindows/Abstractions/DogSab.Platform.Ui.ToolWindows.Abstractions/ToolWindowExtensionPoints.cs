using DogSab.Platform.Extensibility.Abstractions.ExtensionPoints;

namespace DogSab.Platform.Ui.ToolWindows.Abstractions;

/// <summary>
/// Declares the platform's tool window extension point. Application-scoped
/// (not project-scoped) — tool window factories are registered once per
/// process, since a factory itself is stateless; individual open tool window
/// instances are what get created and destroyed per project session, not the
/// factory registration itself.
/// </summary>
public static class ToolWindowExtensionPoints
{
    public static readonly ExtensionPointName<IToolWindowFactory> TOOL_WINDOW =
        ExtensionPointName<IToolWindowFactory>.Create(
            "ui.toolWindow",
            "Contributes a dockable tool window panel to the main application window.");
}