using DogSab.Platform.ProjectModel.Abstractions.Roots;

namespace DogSab.Platform.ProjectModel.Abstractions.Module;

/// <summary>
/// A single buildable unit within a project — the level at which
/// <see cref="ModuleDependency"/> declarations and <see cref="IContentRoot"/>
/// content roots are attached. Roughly corresponds to a single <c>.csproj</c>
/// once a concrete build-system plugin (e.g. MSBuild) maps its own project
/// files onto this abstract model; the platform itself has no notion of what
/// a <c>.csproj</c> is.
/// </summary>
public interface IModule
{
    /// <summary>The module's stable identifier.</summary>
    ModuleId Id { get; }

    /// <summary>A human-readable display name, shown in the Project View and elsewhere in the UI.</summary>
    string DisplayName { get; }

    /// <summary>The content roots (directories) this module's files live under.</summary>
    IReadOnlyList<IContentRoot> ContentRoots { get; }

    /// <summary>Other modules this module depends on.</summary>
    IReadOnlyList<ModuleDependency> Dependencies { get; }
}