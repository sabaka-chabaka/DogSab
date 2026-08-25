namespace DogSab.Platform.Editor.Refactoring;

/// <summary>
/// A single refactoring operation (e.g. Rename, Extract Method), implemented
/// per language and registered against
/// <see cref="Events.RefactoringExtensionPoints.REFACTORING"/>.
/// Split into <see cref="IsApplicable"/>, <see cref="ComputePreview"/> as
/// two separate steps rather than one combined "just do it" method, so the
/// platform can show the user exactly what would change before committing
/// to it — mirroring how real refactoring tools work, rather than
/// irreversibly mutating the document the moment the user invokes the
/// refactoring.
/// </summary>
public interface IRefactoring
{
    /// <summary>
    /// A stable identifier for this refactoring (e.g.
    /// <c>"csharp.rename"</c>), used to reference it from a menu action or
    /// keyboard shortcut binding.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// A human-readable display name, shown in the refactoring menu.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Determines whether this refactoring can meaningfully act at the
    /// given context — e.g. Rename is only applicable when the caret is on
    /// a renameable identifier, not on arbitrary whitespace.
    /// </summary>
    /// <param name="context">
    /// The context to check applicability against.
    /// </param>
    /// <returns>
    /// <c>true</c> if this refactoring can act at this context; otherwise
    /// <c>false</c>.
    /// </returns>
    bool IsApplicable(RefactoringContext context);

    /// <summary>
    /// Computes the full set of proposed edits this refactoring would make,
    /// for preview before the user confirms. Only called when
    /// <see cref="IsApplicable"/> returned <c>true</c> for the same context.
    /// </summary>
    /// <param name="context">
    /// The context to compute the refactoring's proposed changes for.
    /// </param>
    /// <returns>
    /// The proposed preview, including a summary and the list of edits.
    /// </returns>
    RefactoringPreview ComputePreview(RefactoringContext context);
}