using DogSab.Platform.Editor.Highlighting;

namespace DogSab.Platform.Ui.Themes;

/// <summary>
/// Tracks the set of available themes and which one is currently active.
/// Applying a theme to actual Avalonia resources is deliberately out of
/// scope here — this manager is pure state; a future consumer in
/// <c>Ui.Shell</c> would subscribe to <see cref="ActiveThemeChanged"/> and
/// push colors into Avalonia's resource dictionary.
/// </summary>
public sealed class ThemeManagerImpl
{
    private readonly Dictionary<string, ITheme> _themesById = new();

    /// <summary>Raised when <see cref="ActiveTheme"/> changes.</summary>
    public event Action<ITheme>? ActiveThemeChanged;

    /// <summary>The currently active theme.</summary>
    public ITheme ActiveTheme { get; private set; }
    
    /// <summary>
    /// Creates a new theme manager, registering a built-in default theme
    /// immediately so <see cref="ActiveTheme"/> is never in an unset state.
    /// </summary>
    public ThemeManagerImpl()
    {
        var defaultDark = new DefaultDarkTheme();
        _themesById[defaultDark.Id] = defaultDark;
        ActiveTheme = defaultDark;
    }
    
    /// <summary>Registers an additional available theme (e.g. contributed by a plugin).</summary>
    /// <param name="theme">The theme to register.</param>
    public void Register(ITheme theme)
    {
        _themesById[theme.Id] = theme;
    }

    /// <summary>All currently registered themes.</summary>
    public IReadOnlyCollection<ITheme> AllThemes => _themesById.Values;

    /// <summary>
    /// Switches the active theme.
    /// </summary>
    /// <param name="themeId">The ID of the theme to activate.</param>
    /// <exception cref="InvalidOperationException">Thrown if no theme is registered under this ID.</exception>
    public void SetActive(string themeId)
    {
        if (!_themesById.TryGetValue(themeId, out var theme))
        {
            throw new InvalidOperationException($"No theme registered under id '{themeId}'.");
        }

        ActiveTheme = theme;
        ActiveThemeChanged?.Invoke(theme);
    }

    /// <summary>The platform's built-in default dark theme, always available even with no plugins installed.</summary>
    private sealed class DefaultDarkTheme : ITheme
    {
        private static readonly Dictionary<string, string> CategoryColors = new()
        {
            ["csharp.keyword"] = "#569CD6",
            ["csharp.stringLiteral"] = "#CE9178",
            ["csharp.comment"] = "#6A9955",
            ["csharp.identifier"] = "#D4D4D4",
            ["csharp.numberLiteral"] = "#B5CEA8"
        };

        public string Id => "dogsab.dark";
        public string DisplayName => "DogSab Dark";
        public string BackgroundColor => "#1E1E1E";
        public string ForegroundColor => "#D4D4D4";
        public string AccentColor => "#569CD6";

        public string ResolveCategoryColor(ColorCategory category)
        {
            return CategoryColors.TryGetValue(category.Value, out var color) ? color : ForegroundColor;
        }
    }
}