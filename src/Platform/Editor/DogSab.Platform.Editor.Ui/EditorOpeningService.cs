using DogSab.Platform.Editor.Completion;
using DogSab.Platform.Editor.Folding;
using DogSab.Platform.Editor.Inspections;
using DogSab.Platform.Editor.Highlighting;
using DogSab.Platform.Editor.Session;
using DogSab.Platform.Psi.Caching;
using DogSab.Platform.Ui.Themes;
using DogSab.Platform.Vfs.Abstractions.VirtualFile;

namespace DogSab.Platform.Editor.Ui;

/// <summary>
/// The single place in the platform that knows how to actually open a file
/// for editing end to end: creates an <see cref="EditorSession"/>,
/// constructs and configures an <see cref="EditorView"/> with every
/// coordinator it needs, and hands the result to
/// <see cref="IEditorTabsHost"/> to display as a tab.
/// This is the missing integration point flagged when
/// <c>EditorUiStartupActivity</c> was first written — without a component
/// like this, every previously built piece (sessions, coordinators, the
/// view itself, the tabs host) existed in isolation with nothing wiring
/// them together into an actual "open a file" user action.
/// </summary>
public sealed class EditorOpeningService
{
    private readonly IEditorTabsHost _tabsHost;
    private readonly HighlightingCoordinator _highlightingCoordinator;
    private readonly CompletionCoordinator _completionCoordinator;
    private readonly InspectionCoordinator _inspectionCoordinator;
    private readonly FoldingCoordinator _foldingCoordinator;
    private readonly PsiFileCache _psiFileCache;
    private readonly ThemeManagerImpl _themeManager;

    /// <summary>
    /// Creates a new editor opening service.
    /// </summary>
    /// <param name="tabsHost">
    /// The tabs host to open new files into.
    /// </param>
    /// <param name="highlightingCoordinator">
    /// Passed through to every opened <see cref="EditorView"/>.
    /// </param>
    /// <param name="completionCoordinator">
    /// Passed through to every opened <see cref="EditorView"/>.
    /// </param>
    /// <param name="inspectionCoordinator">
    /// Passed through to every opened <see cref="EditorView"/>.
    /// </param>
    /// <param name="foldingCoordinator">
    /// Passed through to every opened <see cref="EditorView"/>.
    /// </param>
    /// <param name="psiFileCache">
    /// Passed through to every opened <see cref="EditorView"/>.
    /// </param>
    /// <param name="themeManager">
    /// Passed through to every opened <see cref="EditorView"/>.
    /// </param>
    public EditorOpeningService(
        IEditorTabsHost tabsHost,
        HighlightingCoordinator highlightingCoordinator,
        CompletionCoordinator completionCoordinator,
        InspectionCoordinator inspectionCoordinator,
        FoldingCoordinator foldingCoordinator,
        PsiFileCache psiFileCache,
        ThemeManagerImpl themeManager)
    {
        _tabsHost = tabsHost;
        _highlightingCoordinator = highlightingCoordinator;
        _completionCoordinator = completionCoordinator;
        _inspectionCoordinator = inspectionCoordinator;
        _foldingCoordinator = foldingCoordinator;
        _psiFileCache = psiFileCache;
        _themeManager = themeManager;
    }

    /// <summary>
    /// Opens a file for editing: creates its session and view (or activates
    /// an already-open tab for the same file), and shows it in the tabs host.
    /// </summary>
    /// <param name="file">
    /// The file to open.
    /// </param>
    public void OpenFile(IVirtualFile file)
    {
        var session = new EditorSession(file);

        var view = new EditorView();
        view.Configure(
            _highlightingCoordinator,
            _completionCoordinator,
            _inspectionCoordinator,
            _foldingCoordinator,
            _psiFileCache,
            _themeManager);
        view.OpenSession(session);

        _tabsHost.OpenTab(file.Path, file.Name, view);
    }
}