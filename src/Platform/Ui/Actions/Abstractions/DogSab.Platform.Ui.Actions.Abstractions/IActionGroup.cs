namespace DogSab.Platform.Ui.Actions.Abstractions;

/// <summary>
/// A named, ordered collection of actions (and/or nested groups), used to
/// build a menu or toolbar.
/// </summary>
public interface IActionGroup
{
    /// <summary>A stable identifier for this group (e.g. <c>"MainMenu.File"</c>).</summary>
    string Id { get; }

    /// <summary>The display text shown for this group (e.g. <c>"File"</c> for the File menu).</summary>
    string DisplayText { get; }

    /// <summary>The group's direct children, in display order.</summary>
    IReadOnlyList<ActionGroupEntry> Children { get; }
}