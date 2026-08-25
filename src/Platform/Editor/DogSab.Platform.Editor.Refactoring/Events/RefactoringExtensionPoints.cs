using DogSab.Platform.Extensibility.Abstractions.ExtensionPoints;

namespace DogSab.Platform.Editor.Refactoring.Events;

/// <summary>
/// Declares the platform's refactoring extension point. Application-scoped
/// — a language plugin registers its refactorings once for the whole
/// process, not per open project.
/// </summary>
public static class RefactoringExtensionPoints
{
    /// <summary>
    /// Contributes a refactoring operation for a specific language.
    /// </summary>
    public static readonly ExtensionPointName<IRefactoring> REFACTORING =
        ExtensionPointName<IRefactoring>.Create(
            "editor.refactoring",
            "Contributes a refactoring operation (e.g. Rename, Extract Method) for a specific language.");
}