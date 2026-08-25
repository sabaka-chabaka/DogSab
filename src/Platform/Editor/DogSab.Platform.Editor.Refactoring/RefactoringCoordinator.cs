using DogSab.Platform.Core.Abstractions.Logging;
using DogSab.Platform.Editor.Abstractions.Document;
using DogSab.Platform.Editor.Refactoring.Events;
using DogSab.Platform.Extensibility.Abstractions.ExtensionPoints;

namespace DogSab.Platform.Editor.Refactoring;

/// <summary>
/// Finds which registered refactorings are applicable at a given context,
/// and applies a chosen refactoring's confirmed preview to a document.
/// </summary>
public sealed class RefactoringCoordinator
{
    /// <summary>
    /// The platform's extension point registry, used to look up every
    /// currently registered <see cref="IRefactoring"/>.
    /// </summary>
    private readonly IExtensionPointRegistry _extensionPointRegistry;

    /// <summary>
    /// Logger used to report an individual refactoring's applicability
    /// check failure without aborting the search for other applicable
    /// refactorings.
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// Creates a new refactoring coordinator.
    /// </summary>
    /// <param name="extensionPointRegistry">
    /// The registry to resolve refactorings from.
    /// </param>
    /// <param name="loggerFactory">
    /// Factory used to obtain a logger scoped to this coordinator.
    /// </param>
    public RefactoringCoordinator(IExtensionPointRegistry extensionPointRegistry, ILoggerFactory loggerFactory)
    {
        _extensionPointRegistry = extensionPointRegistry;
        _logger = loggerFactory.GetLogger(typeof(RefactoringCoordinator));
    }

    /// <summary>
    /// Finds every currently registered refactoring that reports itself
    /// applicable at the given context, e.g. to populate a right-click
    /// context menu of available refactorings at the caret.
    /// </summary>
    /// <param name="context">
    /// The context to check applicability against.
    /// </param>
    /// <returns>
    /// Every refactoring that reported itself applicable, in no
    /// particular guaranteed order.
    /// </returns>
    public IReadOnlyList<IRefactoring> FindApplicable(RefactoringContext context)
    {
        var results = new List<IRefactoring>();

        foreach (var refactoring in _extensionPointRegistry.GetExtensions(RefactoringExtensionPoints.REFACTORING))
        {
            try
            {
                if (refactoring.IsApplicable(context))
                {
                    results.Add(refactoring);
                }
            }
            catch (System.Exception ex)
            {
                _logger.Error(
                    "Refactoring '{0}' failed its applicability check",
                    ex,
                    refactoring.Id);
            }
        }

        return results;
    }

    /// <summary>
    /// Applies a confirmed refactoring preview's edits to a document.
    /// Edits are applied in descending order of offset — last-to-first in
    /// document order — so that applying an earlier edit does not shift
    /// the offsets recorded for edits later in the original list, which
    /// were computed against the document's pre-edit state.
    /// </summary>
    /// <param name="document">
    /// The document to apply the refactoring's edits to.
    /// </param>
    /// <param name="preview">
    /// The confirmed preview whose edits should be applied.
    /// </param>
    public void Apply(IDocument document, RefactoringPreview preview)
    {
        var editsInDescendingOffsetOrder = preview.Edits
            .OrderByDescending(edit => edit.Offset)
            .ToList();

        foreach (var edit in editsInDescendingOffsetOrder)
        {
            document.Replace(edit.Offset, edit.Length, edit.NewText);
        }
    }
}