namespace DogSab.Platform.Ui.Shell.Docking;

/// <summary>
/// Anything that can be docked into the main window's layout — currently
/// only tool windows, but kept as a separate abstraction from
/// <see cref="ToolWindows.Abstractions.IToolWindow"/> so the docking system
/// itself doesn't need a hard dependency on the ToolWindows module; a future
/// dockable panel type (e.g. a detached editor tab group) could implement
/// this without touching ToolWindows at all.
/// </summary>
public interface IDockPanel
{
    /// <summary>A stable identifier for this panel, used for layout persistence.</summary>
    string Id { get; }

    /// <summary>The display title shown in the panel's tab/header.</summary>
    string Title { get; }

    /// <summary>The panel's content, as an opaque object — expected to be an Avalonia <c>Control</c> at the point it's actually hosted.</summary>
    object Content { get; }
}