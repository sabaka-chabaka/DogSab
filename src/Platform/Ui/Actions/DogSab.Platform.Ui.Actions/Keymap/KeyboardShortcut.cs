namespace DogSab.Platform.Ui.Actions.Keymap;

/// <summary>
/// A combination of modifier keys and a primary key that triggers an action
/// (e.g. Ctrl+Shift+N). Stores modifiers as flags so a single shortcut can
/// require multiple held modifiers simultaneously.
/// </summary>
public readonly struct KeyboardShortcut : IEquatable<KeyboardShortcut>
{
    /// <summary>The non-modifier key that must be pressed (e.g. <c>"N"</c>, <c>"F5"</c>).</summary>
    public string Key { get; }

    /// <summary>The modifier keys that must be held simultaneously.</summary>
    public KeyModifiers Modifiers { get; }

    /// <summary>
    /// Creates a new keyboard shortcut.
    /// </summary>
    /// <param name="key">The primary key.</param>
    /// <param name="modifiers">The required modifier keys.</param>
    public KeyboardShortcut(string key, KeyModifiers modifiers = KeyModifiers.None)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Key must not be null or empty.", nameof(key));
        }

        Key = key;
        Modifiers = modifiers;
    }

    /// <inheritdoc />
    public bool Equals(KeyboardShortcut other) =>
        string.Equals(Key, other.Key, StringComparison.OrdinalIgnoreCase) && Modifiers == other.Modifiers;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is KeyboardShortcut other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Key.ToUpperInvariant(), Modifiers);

    /// <inheritdoc />
    public override string ToString()
    {
        var parts = new System.Collections.Generic.List<string>();
        if (Modifiers.HasFlag(KeyModifiers.Control)) parts.Add("Ctrl");
        if (Modifiers.HasFlag(KeyModifiers.Shift)) parts.Add("Shift");
        if (Modifiers.HasFlag(KeyModifiers.Alt)) parts.Add("Alt");
        if (Modifiers.HasFlag(KeyModifiers.Meta)) parts.Add("Meta");
        parts.Add(Key);
        return string.Join("+", parts);
    }

    /// <summary>Equality operator, delegating to <see cref="Equals(KeyboardShortcut)"/>.</summary>
    public static bool operator ==(KeyboardShortcut left, KeyboardShortcut right) => left.Equals(right);

    /// <summary>Inequality operator, delegating to <see cref="Equals(KeyboardShortcut)"/>.</summary>
    public static bool operator !=(KeyboardShortcut left, KeyboardShortcut right) => !left.Equals(right);
}

/// <summary>Modifier keys that can be combined with a primary key in a <see cref="KeyboardShortcut"/>.</summary>
[Flags]
public enum KeyModifiers
{
    None = 0,
    Control = 1 << 0,
    Shift = 1 << 1,
    Alt = 1 << 2,
    Meta = 1 << 3
}