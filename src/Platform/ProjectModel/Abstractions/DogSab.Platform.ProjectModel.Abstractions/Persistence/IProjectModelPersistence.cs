using DogSab.Platform.ProjectModel.Abstractions.Solution;

namespace DogSab.Platform.ProjectModel.Abstractions.Persistence;

/// <summary>
/// Contract for saving and loading the structural project model (solution,
/// projects, modules, content roots) to and from some persistent form. The
/// platform itself has no opinion on the on-disk format — a concrete
/// implementation (e.g. <c>XmlProjectModelPersistence</c> in the Impl
/// assembly, or a future MSBuild-aware plugin that maps <c>.sln</c>/<c>.csproj</c>
/// directly) decides that. Kept separate from <c>Core.Abstractions.Settings.ISettingsStore</c>,
/// since that persists arbitrary plain settings objects, while this persists
/// a specific, structured object graph with its own identity and referential
/// relationships (a project's modules, a module's dependencies on other
/// modules by ID) that a generic settings serializer is not designed to preserve.
/// </summary>
public interface IProjectModelPersistence
{
    /// <summary>
    /// Loads a solution's structure from its persisted form.
    /// </summary>
    /// <param name="solutionFilePath">The path to the persisted solution file.</param>
    /// <param name="cancellationToken">Token used to cancel a long-running load.</param>
    /// <returns>The loaded solution structure.</returns>
    Task<ISolution> LoadAsync(string solutionFilePath, CancellationToken cancellationToken);

    /// <summary>
    /// Persists a solution's current structure.
    /// </summary>
    /// <param name="solution">The solution to persist.</param>
    /// <param name="solutionFilePath">The path to write the persisted solution file to.</param>
    /// <param name="cancellationToken">Token used to cancel a long-running save.</param>
    /// <returns>A task that completes when the save has finished.</returns>
    Task SaveAsync(ISolution solution, string solutionFilePath, CancellationToken cancellationToken);
}
