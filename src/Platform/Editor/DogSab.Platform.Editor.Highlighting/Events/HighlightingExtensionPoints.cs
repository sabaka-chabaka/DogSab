using DogSab.Platform.Extensibility.Abstractions.ExtensionPoints;

namespace DogSab.Platform.Editor.Highlighting.Events;

/// <summary>
/// Declares the platform's syntax highlighting extension point.
/// Application-scoped — a language plugin registers one highlighter for the
/// whole process, not per open project.
/// </summary>
public static class HighlightingExtensionPoints
{
    /// <summary>
    /// Contributes syntax highlighting computation for a specific language.
    /// </summary>
    public static readonly ExtensionPointName<ISyntaxHighlighter> SYNTAX_HIGHLIGHTER =
        ExtensionPointName<ISyntaxHighlighter>.Create(
            "editor.syntaxHighlighter",
            "Computes syntax highlighting spans for a specific language.");
}