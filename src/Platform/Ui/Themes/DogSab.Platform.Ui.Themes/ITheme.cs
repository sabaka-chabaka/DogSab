namespace DogSab.Platform.Ui.Themes;

/// <summary>
/// A color/visual scheme for the application (e.g. "Dark", "Light"). Kept
/// deliberately minimal at this stage — a handful of named colors rather
/// than a full styling system — since deeper Avalonia theming (control
/// templates, resource dictionaries) is a substantially larger undertaking
/// best deferred until there's an actual Editor to theme.
/// </summary>
public interface ITheme
{
    /// <summary>A stable identifier for this theme (e.g. <c>"dark"</c>).</summary>
    string Id { get; }

    /// <summary>A human-readable display name, shown in the theme picker.</summary>
    string DisplayName { get; }

    /// <summary>The primary background color, as a hex string (e.g. <c>"#1E1E1E"</c>).</summary>
    string BackgroundColor { get; }

    /// <summary>The primary foreground/text color.</summary>
    string ForegroundColor { get; }

    /// <summary>The accent color used for selections, highlights, and focus indicators.</summary>
    string AccentColor { get; }
}