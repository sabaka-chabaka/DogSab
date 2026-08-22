namespace DogSab.Platform.Ui.Actions.Abstractions;

/// <summary>
/// A single entry within an <see cref="IActionGroup"/>: either a leaf
/// <see cref="AnAction"/> (a menu item) or a nested <see cref="IActionGroup"/>
/// (a submenu). An explicit wrapper rather than a bare <see cref="object"/>
/// field, so consumers building a menu from a group's children get a
/// compile-time guarantee of exactly these two possibilities — the same
/// rationale as <see cref="Resolve.ResolveResult"/> in Psi.Abstractions being
/// an explicit struct rather than a nullable reference.
/// </summary>
public readonly struct ActionGroupEntry
{
    /// <summary>The leaf action, if this entry wraps one; <c>null</c> if it wraps a subgroup instead.</summary>
    public AnAction? Action { get; }

    /// <summary>The nested subgroup, if this entry wraps one; <c>null</c> if it wraps a leaf action instead.</summary>
    public IActionGroup? SubGroup { get; }

    /// <summary>Whether this entry wraps a leaf action rather than a subgroup.</summary>
    public bool IsAction => Action is not null;

    private ActionGroupEntry(AnAction? action, IActionGroup? subGroup)
    {
        Action = action;
        SubGroup = subGroup;
    }

    /// <summary>Wraps a leaf action as a group entry.</summary>
    /// <param name="action">The action to wrap.</param>
    /// <returns>A new entry wrapping the action.</returns>
    public static ActionGroupEntry FromAction(AnAction action) => new(action, null);

    /// <summary>Wraps a nested subgroup as a group entry.</summary>
    /// <param name="subGroup">The subgroup to wrap.</param>
    /// <returns>A new entry wrapping the subgroup.</returns>
    public static ActionGroupEntry FromSubGroup(IActionGroup subGroup) => new(null, subGroup);

    /// <summary>
    /// Pattern-matches this entry, invoking the matching function and
    /// returning its result — a safer alternative to manually checking
    /// <see cref="IsAction"/> and null-forgiving one of the two properties.
    /// </summary>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="onAction">Called with the wrapped action, if this entry is an action.</param>
    /// <param name="onSubGroup">Called with the wrapped subgroup, if this entry is a subgroup.</param>
    /// <returns>The result of whichever function matched.</returns>
    public T Match<T>(Func<AnAction, T> onAction, Func<IActionGroup, T> onSubGroup)
    {
        return IsAction ? onAction(Action!) : onSubGroup(SubGroup!);
    }
}