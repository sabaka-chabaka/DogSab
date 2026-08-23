using DogSab.Platform.Psi.Abstractions.Tree;

namespace DogSab.Platform.Editor.Abstractions.Folding;

/// <summary>
/// Computes the foldable regions for a file, given its PSI tree. Implemented
/// per language (folding rules are language-specific — what counts as a
/// "foldable" construct differs between C# methods, Python indentation
/// blocks, and JSON objects) and registered against
/// <see cref="Events.EditorExtensionPoints.FOLDING_PROVIDER"/>. Takes the
/// already-parsed <see cref="IPsiFile"/> rather than raw text, so folding
/// reuses the same tree Indexing and other Psi-consuming features already
/// paid the cost to build, instead of re-scanning the file's text independently.
/// </summary>
public interface IFoldingProvider
{
    /// <summary>
    /// Computes the foldable regions for a file.
    /// </summary>
    /// <param name="psiFile">The file's parsed PSI tree.</param>
    /// <returns>The foldable regions found, in document order.</returns>
    IEnumerable<IFoldingRegion> ComputeFoldingRegions(IPsiFile psiFile);
}