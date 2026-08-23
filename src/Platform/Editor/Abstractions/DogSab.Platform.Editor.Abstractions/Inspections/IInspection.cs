using DogSab.Platform.Psi.Abstractions.Tree;

namespace DogSab.Platform.Editor.Abstractions.Inspections;

/// <summary>
/// Analyzes a parsed file and reports diagnostic problems (errors, warnings,
/// suggestions), rendered as squiggly underlines in the editor. Implemented
/// per language and registered against
/// <see cref="Events.EditorExtensionPoints.INSPECTION"/>. Multiple inspections
/// may run against the same file (e.g. "unused variable", "missing
/// semicolon" as separate inspections rather than one monolithic checker),
/// each contributing its own findings independently.
/// </summary>
public interface IInspection
{
    /// <summary>A stable identifier for this inspection, so users can configure it individually (enable/disable, change severity) in a future settings UI.</summary>
    string Id { get; }

    /// <summary>A human-readable display name, shown in inspection settings.</summary>
    string DisplayName { get; }

    /// <summary>
    /// Analyzes a file and reports any problems found.
    /// </summary>
    /// <param name="psiFile">The file's parsed PSI tree.</param>
    /// <returns>The problems found, in no particular order.</returns>
    IEnumerable<Problem> Analyze(IPsiFile psiFile);
}