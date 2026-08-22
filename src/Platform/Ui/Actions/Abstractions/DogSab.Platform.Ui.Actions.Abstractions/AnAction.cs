namespace DogSab.Platform.Ui.Actions.Abstractions;

/// <summary>
/// Base class for a single invokable action (e.g. a menu item, toolbar
/// button, or keyboard shortcut target). An abstract class rather than an
/// interface — unlike most platform extension contracts — because actions
/// share genuinely common state and default behavior (display text, default
/// enablement) that would otherwise be duplicated by every implementation;
/// see the earlier discussion on this exact tradeoff.
/// </summary>
public abstract class AnAction
{
    /// <summary>
    /// Creates a new action.
    /// </summary>
    /// <param name="displayText">The text shown in menus/toolbars for this action.</param>
    /// <param name="description">A longer description, shown as a tooltip.</param>
    protected AnAction(string displayText, string description = "")
    {
        DisplayText = displayText;
        Description = description;
    }

    /// <summary>The text shown in menus, toolbars, and the Search Everywhere action list.</summary>
    public string DisplayText { get; }

    /// <summary>A longer description, shown as a tooltip when hovering the action.</summary>
    public string Description { get; }

    /// <summary>
    /// Determines whether this action is currently enabled, given the
    /// current context. Default implementation always returns <c>true</c>;
    /// override to disable the action when it doesn't apply (e.g. "Build
    /// Project" disabled when no project is open).
    /// </summary>
    /// <param name="context">The current action context.</param>
    /// <returns><c>true</c> if the action should be enabled; otherwise <c>false</c>.</returns>
    public virtual bool IsEnabled(ActionContext context) => true;

    /// <summary>
    /// Runs the action's behavior. Only called when <see cref="IsEnabled"/>
    /// returned <c>true</c> for the same context.
    /// </summary>
    /// <param name="context">The current action context.</param>
    public abstract void Execute(ActionContext context);
}