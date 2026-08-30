using DogSab.Platform.Editor.Ui;
using DogSab.Platform.ProjectModel.Module;
using DogSab.Platform.Ui.ToolWindows.Abstractions;

namespace DogSab.Platform.Ui.Shell.ProjectView;

/// <summary>
/// Factory for the Project View tool window, wiring its double-click
/// activation directly to <see cref="EditorOpeningService.OpenFile"/> —
/// closing the loop from "see a file in the tree" to "it opens in an
/// editor tab" that was the whole point of building this tool window.
/// </summary>
public sealed class ProjectViewToolWindowFactory : IToolWindowFactory
{
    private readonly ProjectModelManager _projectModelManager;
    private readonly EditorOpeningService _openingService;

    /// <summary>
    /// Creates a new Project View tool window factory.
    /// </summary>
    /// <param name="projectModelManager">
    /// Used to resolve the current solution's first project's first
    /// module's first content root as the tree's displayed root — a
    /// simplification pending a real "active project" concept for
    /// multi-project workspaces.
    /// </param>
    /// <param name="openingService">
    /// Used to open a double-clicked file for editing.
    /// </param>
    public ProjectViewToolWindowFactory(ProjectModelManager projectModelManager, EditorOpeningService openingService)
    {
        _projectModelManager = projectModelManager;
        _openingService = openingService;
    }

    /// <inheritdoc />
    public string ToolWindowId => "dogsab.projectView";

    /// <inheritdoc />
    public string Title => "Project";

    /// <inheritdoc />
    public IToolWindow Create()
    {
        var view = new ProjectViewToolWindow();
        view.FileActivated += _openingService.OpenFile;

        var rootDirectory = ResolveDisplayedRoot();
        if (rootDirectory is not null)
        {
            view.SetRoot(rootDirectory);
        }

        return new ProjectViewToolWindowWrapper(view);
    }

    /// <summary>
    /// Resolves which directory to show as the tree's root.
    /// A known simplification: always the first content root of the first
    /// module of the first project in the current solution, rather than
    /// respecting a real multi-project "active project" selection.
    /// </summary>
    /// <returns>
    /// The resolved root directory, or <c>null</c> if the current solution
    /// has no projects, modules, or content roots yet.
    /// </returns>
    private Vfs.Abstractions.VirtualFile.IVirtualFile? ResolveDisplayedRoot()
    {
        var solution = _projectModelManager.CurrentSolution;
        var firstProject = solution.Projects.Count > 0 ? solution.Projects[0] : null;
        var firstModule = firstProject?.Modules.Count > 0 ? firstProject.Modules[0] : null;
        var firstContentRoot = firstModule?.ContentRoots.Count > 0 ? firstModule.ContentRoots[0] : null;

        return firstContentRoot?.RootDirectory;
    }

    /// <summary>
    /// Wraps a <see cref="ProjectViewToolWindow"/> control as an
    /// <see cref="IToolWindow"/>.
    /// </summary>
    private sealed class ProjectViewToolWindowWrapper : IToolWindow
    {
        private readonly ProjectViewToolWindow _view;

        public ProjectViewToolWindowWrapper(ProjectViewToolWindow view) => _view = view;

        public string Id => "dogsab.projectView";
        public string Title => "Project";
        public ToolWindowAnchor DefaultAnchor => ToolWindowAnchor.Left;
        public object Content => _view;
    }
}