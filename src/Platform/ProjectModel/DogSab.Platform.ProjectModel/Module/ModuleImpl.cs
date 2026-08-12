using DogSab.Platform.ProjectModel.Abstractions.Module;
using DogSab.Platform.ProjectModel.Abstractions.Roots;

namespace DogSab.Platform.ProjectModel.Module;

/// <summary>
/// Default, immutable implementation of <see cref="IModule"/>.
/// </summary>
public sealed class ModuleImpl : IModule
{
    /// <inheritdoc />
    public ModuleId Id { get; }

    /// <inheritdoc />
    public string DisplayName { get; }

    /// <inheritdoc />
    public IReadOnlyList<IContentRoot> ContentRoots { get; }

    /// <inheritdoc />
    public IReadOnlyList<ModuleDependency> Dependencies { get; }

    /// <summary>
    /// Creates a new module.
    /// </summary>
    /// <param name="id">The module's stable identifier.</param>
    /// <param name="displayName">A human-readable display name.</param>
    /// <param name="contentRoots">The content roots this module's files live under.</param>
    /// <param name="dependencies">Other modules this module depends on.</param>
    public ModuleImpl(
        ModuleId id,
        string displayName,
        IReadOnlyList<IContentRoot> contentRoots,
        IReadOnlyList<ModuleDependency> dependencies)
    {
        Id = id;
        DisplayName = displayName;
        ContentRoots = contentRoots;
        Dependencies = dependencies;
    }
}