using DogSab.Platform.Editor.Highlighting;

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
    
    /// <summary>
    /// Resolves the display color for a specific syntax highlighting
    /// category (e.g. <c>"csharp.keyword"</c>), falling back to
    /// <see cref="ForegroundColor"/> for any category the theme has no
    /// specific color for — added to close the gap where
    /// <c>Editor.Ui.Rendering.EditorTextRenderer</c> previously rendered
    /// every highlighted span in the same default foreground color
    /// regardless of its category, making syntax highlighting compute
    /// correctly but display with no visible effect.
    /// </summary>
    /// <param name="category">
    /// The highlighting category to resolve a color for.
    /// </param>
    /// <returns>
    /// The color to render text of this category in, as a hex string.
    /// </returns>
    string ResolveCategoryColor(ColorCategory category);
}