using DogSab.Platform.Core.Abstractions.Logging;
using DogSab.Platform.Editor.Highlighting.Events;
using DogSab.Platform.Extensibility.Abstractions.ExtensionPoints;
using DogSab.Platform.Psi.Abstractions.Tree;

namespace DogSab.Platform.Editor.Highlighting;

/// <summary>
/// Resolves and invokes every registered <see cref="ISyntaxHighlighter"/>
/// for a file, aggregating their highlight spans.
/// A single highlighter throwing is caught and logged rather than allowed
/// to abort the whole computation — the same defensive approach already
/// used in <c>Editor.Inspections.InspectionCoordinator</c>, applied here
/// consistently rather than repeating the earlier omission in
/// <c>FoldingCoordinator</c>/<c>CompletionCoordinator</c>.
/// </summary>
public sealed class HighlightingCoordinator
{
    /// <summary>
    /// The platform's extension point registry, used to look up every
    /// currently registered <see cref="ISyntaxHighlighter"/>.
    /// </summary>
    private readonly IExtensionPointRegistry _extensionPointRegistry;

    /// <summary>
    /// Logger used to report an individual highlighter's failure without
    /// aborting the rest of the computation.
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// Creates a new highlighting coordinator.
    /// </summary>
    /// <param name="extensionPointRegistry">
    /// The registry to resolve syntax highlighters from.
    /// </param>
    /// <param name="loggerFactory">
    /// Factory used to obtain a logger scoped to this coordinator.
    /// </param>
    public HighlightingCoordinator(IExtensionPointRegistry extensionPointRegistry, ILoggerFactory loggerFactory)
    {
        _extensionPointRegistry = extensionPointRegistry;
        _logger = loggerFactory.GetLogger(typeof(HighlightingCoordinator));
    }

    /// <summary>
    /// Computes syntax highlighting for a file by asking every currently
    /// registered highlighter to contribute its spans, catching and
    /// logging any individual highlighter's failure so it does not prevent
    /// the remaining highlighters from contributing.
    /// </summary>
    /// <param name="psiFile">
    /// The file's parsed PSI tree to compute highlighting for.
    /// </param>
    /// <returns>
    /// Every highlight span contributed across all registered highlighters
    /// that ran successfully.
    /// </returns>
    public IReadOnlyList<HighlightSpan> ComputeHighlighting(IPsiFile psiFile)
    {
        var results = new List<HighlightSpan>();

        foreach (var highlighter in _extensionPointRegistry.GetExtensions(HighlightingExtensionPoints.SYNTAX_HIGHLIGHTER))
        {
            try
            {
                results.AddRange(highlighter.ComputeHighlighting(psiFile));
            }
            catch (System.Exception ex)
            {
                _logger.Error(
                    "Syntax highlighter '{0}' failed while highlighting file '{1}'",
                    ex,
                    highlighter.GetType().FullName,
                    psiFile.VirtualFile.Path);
            }
        }

        return results;
    }
}