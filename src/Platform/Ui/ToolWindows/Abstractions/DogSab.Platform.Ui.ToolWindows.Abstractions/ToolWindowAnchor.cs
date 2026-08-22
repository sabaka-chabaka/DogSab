namespace DogSab.Platform.Ui.ToolWindows.Abstractions;

/// <summary>
/// The edge of the main window a tool window docks to by default. The user
/// can typically drag a tool window to a different anchor at runtime — this
/// only determines where it first appears when registered.
/// </summary>
public enum ToolWindowAnchor
{
    /// <summary>Docked to the left edge (e.g. Project View).</summary>
    Left,

    /// <summary>Docked to the right edge (e.g. a future Structure view).</summary>
    Right,

    /// <summary>Docked to the bottom edge (e.g. a future Debug or Build Output panel).</summary>
    Bottom
}