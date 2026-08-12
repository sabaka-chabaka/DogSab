using System.Xml.Serialization;
using DogSab.Platform.Core.Abstractions.Logging;
using DogSab.Platform.ProjectModel.Abstractions.Module;
using DogSab.Platform.ProjectModel.Abstractions.Persistence;
using DogSab.Platform.ProjectModel.Abstractions.Project;
using DogSab.Platform.ProjectModel.Abstractions.Roots;
using DogSab.Platform.ProjectModel.Abstractions.Solution;
using DogSab.Platform.ProjectModel.Module;
using DogSab.Platform.ProjectModel.Project;
using DogSab.Platform.ProjectModel.Roots;
using DogSab.Platform.ProjectModel.Solution;
using DogSab.Platform.Vfs.Abstractions.Exceptions;
using DogSab.Platform.Vfs.FileSystem;

namespace DogSab.Platform.ProjectModel.Persistence;

public sealed class XmlProjectModelPersistence : IProjectModelPersistence
{
    private readonly VirtualFileSystemRouter _vfsRouter;
    private readonly ILogger _logger;

    /// <summary>
    /// Creates a new XML-based project model persistence provider.
    /// </summary>
    /// <param name="vfsRouter">Router used to re-resolve stored virtual paths back into <see cref="Vfs.Abstractions.VirtualFile.IVirtualFile"/> instances on load.</param>
    /// <param name="loggerFactory">Factory used to obtain a logger for reporting per-module load failures without aborting the whole solution.</param>
    public XmlProjectModelPersistence(VirtualFileSystemRouter vfsRouter, ILoggerFactory loggerFactory)
    {
        _vfsRouter = vfsRouter;
        _logger = loggerFactory.GetLogger(typeof(XmlProjectModelPersistence));
    }

    /// <inheritdoc />
    public async Task<ISolution> LoadAsync(string solutionFilePath, CancellationToken cancellationToken)
    {
        var serializer = new XmlSerializer(typeof(SolutionXmlModel));

        await using var stream = File.OpenRead(solutionFilePath);
        var xmlModel = (SolutionXmlModel?)serializer.Deserialize(stream)
            ?? throw new InvalidDataException($"Solution file '{solutionFilePath}' deserialized to null.");

        cancellationToken.ThrowIfCancellationRequested();

        var projects = xmlModel.Projects
            .Select(p => TryMapProject(p, solutionFilePath))
            .Where(p => p is not null)
            .Select(p => p!)
            .ToList();

        return new SolutionImpl(new SolutionId(xmlModel.Id), xmlModel.DisplayName, projects);
    }

    /// <summary>
    /// Attempts to map a project's XML entry to a live <see cref="IProject"/>.
    /// If any of the project's modules fail to map (e.g. a stored content
    /// root path no longer exists on disk), that module is skipped with a
    /// logged warning rather than aborting the entire solution load; if the
    /// whole project fails unexpectedly, the project itself is skipped and
    /// <c>null</c> is returned, so one broken project does not prevent the
    /// rest of the solution from loading.
    /// </summary>
    private IProject? TryMapProject(ProjectXmlModel xml, string solutionFilePath)
    {
        try
        {
            var modules = xml.Modules
                .Select(m => TryMapModule(m, xml.DisplayName))
                .Where(m => m is not null)
                .Select(m => m!)
                .ToList();

            return new ProjectImpl(new ProjectId(xml.Id), xml.DisplayName, modules);
        }
        catch (Exception ex)
        {
            _logger.Error(
                "Failed to load project '{0}' from solution '{1}'; skipping it.",
                ex,
                xml.DisplayName,
                solutionFilePath);
            return null;
        }
    }

    /// <summary>
    /// Attempts to map a module's XML entry to a live <see cref="IModule"/>.
    /// If a content root's stored path no longer resolves to an existing
    /// virtual file, the whole module is skipped with a logged warning,
    /// since a module missing its content is not meaningfully usable, but
    /// this does not prevent the rest of the project's modules from loading.
    /// </summary>
    private IModule? TryMapModule(ModuleXmlModel xml, string projectDisplayName)
    {
        try
        {
            var contentRoots = xml.ContentRoots.Select(MapContentRoot).ToList();
            var dependencies = xml.Dependencies
                .Select(d => new ModuleDependency(new ModuleId(d.ModuleId), d.IsExported))
                .ToList();

            return new ModuleImpl(new ModuleId(xml.Id), xml.DisplayName, contentRoots, dependencies);
        }
        catch (VirtualFileNotFoundException ex)
        {
            _logger.Warn(
                "Module '{0}' in project '{1}' references a content root that no longer exists ('{2}'); skipping this module.",
                xml.DisplayName,
                projectDisplayName,
                ex.Path);
            return null;
        }
    }

    private ContentRootImpl MapContentRoot(ContentRootXmlModel xml)
    {
        var rootDirectory = _vfsRouter.Require(xml.RootDirectoryPath);
        var sourceFolders = xml.SourceFolders
            .Select(sf => new SourceFolderImpl(_vfsRouter.Require(sf.DirectoryPath), sf.Type))
            .ToList();

        return new ContentRootImpl(rootDirectory, sourceFolders);
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

// DTOs unchanged — same as before
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