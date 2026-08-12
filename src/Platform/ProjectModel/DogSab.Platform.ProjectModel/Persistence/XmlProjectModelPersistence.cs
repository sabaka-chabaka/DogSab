using System.Xml.Serialization;
using DogSab.Platform.ProjectModel.Abstractions.Module;
using DogSab.Platform.ProjectModel.Abstractions.Persistence;
using DogSab.Platform.ProjectModel.Abstractions.Project;
using DogSab.Platform.ProjectModel.Abstractions.Roots;
using DogSab.Platform.ProjectModel.Abstractions.Solution;
using DogSab.Platform.ProjectModel.Module;
using DogSab.Platform.ProjectModel.Project;
using DogSab.Platform.ProjectModel.Roots;
using DogSab.Platform.ProjectModel.Solution;
using DogSab.Platform.Vfs.FileSystem;

namespace DogSab.Platform.ProjectModel.Persistence;

/// <summary>
/// Default implementation of <see cref="IProjectModelPersistence"/>, storing
/// the solution structure as XML. Serializes only structural references
/// (module IDs for dependencies, virtual paths for content roots) rather than
/// the live <see cref="Abstractions.Module.IModule"/> object graph directly,
/// since the live objects hold references to <see cref="Vfs.Abstractions.VirtualFile.IVirtualFile"/>
/// instances that are not themselves meant to be serialized — they are
/// re-resolved from the stored paths on load, through the platform's VFS router.
/// </summary>
public sealed class XmlProjectModelPersistence : IProjectModelPersistence
{
    private readonly VirtualFileSystemRouter _vfsRouter;

    /// <summary>
    /// Creates a new XML-based project model persistence provider.
    /// </summary>
    /// <param name="vfsRouter">Router used to re-resolve stored virtual paths back into <see cref="Vfs.Abstractions.VirtualFile.IVirtualFile"/> instances on load.</param>
    public XmlProjectModelPersistence(VirtualFileSystemRouter vfsRouter)
    {
        _vfsRouter = vfsRouter;
    }

    /// <inheritdoc />
    public async Task<ISolution> LoadAsync(string solutionFilePath, CancellationToken cancellationToken)
    {
        var serializer = new XmlSerializer(typeof(SolutionXmlModel));

        await using var stream = File.OpenRead(solutionFilePath);
        var xmlModel = (SolutionXmlModel?)serializer.Deserialize(stream)
            ?? throw new InvalidDataException($"Solution file '{solutionFilePath}' deserialized to null.");

        cancellationToken.ThrowIfCancellationRequested();

        var projects = xmlModel.Projects.Select(MapProject).ToList();
        return new SolutionImpl(new SolutionId(xmlModel.Id), xmlModel.DisplayName, projects);
    }

    /// <inheritdoc />
    public async Task SaveAsync(ISolution solution, string solutionFilePath, CancellationToken cancellationToken)
    {
        var xmlModel = new SolutionXmlModel
        {
            Id = solution.Id.Value,
            DisplayName = solution.DisplayName,
            Projects = solution.Projects.Select(MapProjectToXml).ToList()
        };

        cancellationToken.ThrowIfCancellationRequested();

        var serializer = new XmlSerializer(typeof(SolutionXmlModel));
        var tempPath = solutionFilePath + ".tmp";

        await using (var stream = File.Create(tempPath))
        {
            serializer.Serialize(stream, xmlModel);
        }

        File.Move(tempPath, solutionFilePath, overwrite: true);
    }

    private IProject MapProject(ProjectXmlModel xml)
    {
        var modules = xml.Modules.Select(MapModule).ToList();
        return new ProjectImpl(new ProjectId(xml.Id), xml.DisplayName, modules);
    }

    private IModule MapModule(ModuleXmlModel xml)
    {
        var contentRoots = xml.ContentRoots.Select(MapContentRoot).ToList();
        var dependencies = xml.Dependencies
            .Select(d => new ModuleDependency(new ModuleId(d.ModuleId), d.IsExported))
            .ToList();

        return new ModuleImpl(new ModuleId(xml.Id), xml.DisplayName, contentRoots, dependencies);
    }

    private ContentRootImpl MapContentRoot(ContentRootXmlModel xml)
    {
        var rootDirectory = _vfsRouter.Require(xml.RootDirectoryPath);
        var sourceFolders = xml.SourceFolders
            .Select(sf => new SourceFolderImpl(_vfsRouter.Require(sf.DirectoryPath), sf.Type))
            .ToList();

        return new ContentRootImpl(rootDirectory, sourceFolders);
    }

    private static ProjectXmlModel MapProjectToXml(IProject project) => new()
    {
        Id = project.Id.Value,
        DisplayName = project.DisplayName,
        Modules = project.Modules.Select(MapModuleToXml).ToList()
    };

    private static ModuleXmlModel MapModuleToXml(IModule module) => new()
    {
        Id = module.Id.Value,
        DisplayName = module.DisplayName,
        Dependencies = module.Dependencies
            .Select(d => new ModuleDependencyXmlModel { ModuleId = d.DependencyModuleId.Value, IsExported = d.IsExported })
            .ToList(),
        ContentRoots = module.ContentRoots.Select(MapContentRootToXml).ToList()
    };

    private static ContentRootXmlModel MapContentRootToXml(IContentRoot root) => new()
    {
        RootDirectoryPath = root.RootDirectory.Path,
        SourceFolders = root.SourceFolders
            .Select(sf => new SourceFolderXmlModel { DirectoryPath = sf.Directory.Path, Type = sf.Type })
            .ToList()
    };
}

// --- Raw XML DTOs, mirroring the pattern already used for plugin manifest JSON DTOs ---

internal sealed class SolutionXmlModel
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public List<ProjectXmlModel> Projects { get; set; } = new();
}

internal sealed class ProjectXmlModel
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public List<ModuleXmlModel> Modules { get; set; } = new();
}

internal sealed class ModuleXmlModel
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public List<ModuleDependencyXmlModel> Dependencies { get; set; } = new();
    public List<ContentRootXmlModel> ContentRoots { get; set; } = new();
}

internal sealed class ModuleDependencyXmlModel
{
    public string ModuleId { get; set; } = string.Empty;
    public bool IsExported { get; set; }
}

internal sealed class ContentRootXmlModel
{
    public string RootDirectoryPath { get; set; } = string.Empty;
    public List<SourceFolderXmlModel> SourceFolders { get; set; } = new();
}

internal sealed class SourceFolderXmlModel
{
    public string DirectoryPath { get; set; } = string.Empty;
    public SourceRootType Type { get; set; }
}