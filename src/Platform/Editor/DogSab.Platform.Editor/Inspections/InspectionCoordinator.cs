using DogSab.Platform.Core.Abstractions.Logging;
using DogSab.Platform.Editor.Abstractions.Events;
using DogSab.Platform.Editor.Abstractions.Inspections;
using DogSab.Platform.Extensibility.Abstractions.ExtensionPoints;
using DogSab.Platform.Psi.Abstractions.Tree;

namespace DogSab.Platform.Editor.Inspections;

/// <summary>
/// Resolves and runs every registered <see cref="IInspection"/> against a
/// file, aggregating their reported problems.
/// Unlike <see cref="Folding.FoldingCoordinator"/> and
/// <see cref="Completion.CompletionCoordinator"/>, a single misbehaving
/// inspection here is explicitly not allowed to prevent the rest of the
/// file's inspections from running — an inspection is arbitrary
/// plugin-contributed analysis code, and one buggy inspection throwing an
/// exception should not silently hide every other inspection's otherwise
/// valid findings for the file.
/// </summary>
public sealed class InspectionCoordinator
{
    /// <summary>
    /// The platform's extension point registry, used to look up every
    /// currently registered <see cref="IInspection"/>.
    /// </summary>
    private readonly IExtensionPointRegistry _extensionPointRegistry;

    /// <summary>
    /// Logger used to report an individual inspection's failure without
    /// aborting the rest of the analysis pass.
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// Creates a new inspection coordinator.
    /// </summary>
    /// <param name="extensionPointRegistry">
    /// The registry to resolve inspections from.
    /// </param>
    /// <param name="loggerFactory">
    /// Factory used to obtain a logger scoped to this coordinator.
    /// </param>
    public InspectionCoordinator(IExtensionPointRegistry extensionPointRegistry, ILoggerFactory loggerFactory)
    {
        _extensionPointRegistry = extensionPointRegistry;
        _logger = loggerFactory.GetLogger(typeof(InspectionCoordinator));
    }

    /// <summary>
    /// Runs every currently registered inspection against a file, catching
    /// and logging any individual inspection's failure so it does not
    /// prevent the remaining inspections from running and contributing
    /// their own findings.
    /// </summary>
    /// <param name="psiFile">
    /// The file's parsed PSI tree to analyze.
    /// </param>
    /// <returns>
    /// Every problem reported across all registered inspections that ran
    /// successfully.
    /// </returns>
    public IReadOnlyList<Problem> Analyze(IPsiFile psiFile)
    {
        var results = new List<Problem>();

        foreach (var inspection in _extensionPointRegistry.GetExtensions(EditorExtensionPoints.INSPECTION))
        {
            try
            {
                results.AddRange(inspection.Analyze(psiFile));
            }
            catch (System.Exception ex)
            {
                _logger.Error(
                    "Inspection '{0}' failed while analyzing file '{1}'",
                    ex,
                    inspection.Id,
                    psiFile.VirtualFile.Path);
            }
        }

        return results;
    }
}