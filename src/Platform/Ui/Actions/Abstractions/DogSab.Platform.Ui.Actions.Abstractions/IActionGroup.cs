namespace DogSab.Platform.Ui.Actions.Abstractions;

/// <summary>
/// A named, ordered collection of actions (and/or nested groups), used to
/// build a menu or toolbar. Unlike <see cref="AnAction"/>, a plain interface
/// is sufficient here — a group has no meaningful default behavior to share,
/// it's purely a structural container.
/// </summary>
public interface IActionGroup
{
    /// <summary>A stable identifier for this group (e.g. <c>"MainMenu.File"</c>), used to reference it from a parent group or the main menu bar definition.</summary>
    string Id { get; }

    /// <summary>The display text shown for this group (e.g. <c>"File"</c> for the File menu).</summary>
    string DisplayText { get; }

    /// <summary>
    /// The group's direct children, in display order. Each entry is either
    /// an <see cref="AnAction"/> (a leaf menu item) or another
    /// <see cref="IActionGroup"/> (a submenu).
    /// </summary>
    IReadOnlyList<object> Children { get; }
}