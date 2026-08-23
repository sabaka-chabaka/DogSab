using DogSab.Platform.Extensibility.Abstractions.ExtensionPoints;
using DogSab.Platform.Editor.Abstractions.Completion;
using DogSab.Platform.Editor.Abstractions.Folding;
using DogSab.Platform.Editor.Abstractions.Inspections;

namespace DogSab.Platform.Editor.Abstractions.Events;

/// <summary>
/// Declares every extension point defined by the Editor module: completion,
/// folding, and inspections. All application-scoped — a language plugin
/// registers one provider per language for the whole process, not per open
/// project, since which languages are installed doesn't vary per project.
/// </summary>
public static class EditorExtensionPoints
{
    /// <summary>Contributes code completion suggestions for a specific language.</summary>
    public static readonly ExtensionPointName<ICompletionProvider> COMPLETION_PROVIDER =
        ExtensionPointName<ICompletionProvider>.Create(
            "editor.completionProvider",
            "Provides code completion suggestions for a specific language.");

    /// <summary>Contributes foldable region detection for a specific language.</summary>
    public static readonly ExtensionPointName<IFoldingProvider> FOLDING_PROVIDER =
        ExtensionPointName<IFoldingProvider>.Create(
            "editor.foldingProvider",
            "Computes foldable code regions for a specific language.");

    /// <summary>Contributes a diagnostic inspection for a specific language.</summary>
    public static readonly ExtensionPointName<IInspection> INSPECTION =
        ExtensionPointName<IInspection>.Create(
            "editor.inspection",
            "Analyzes parsed files and reports diagnostic problems.");
}