namespace DogSab.Platform.Editor.Ui;

/// <summary>
/// Contract for hosting opened editor tabs, implemented by
/// <c>Ui.Shell.EditorTabsHost</c>. Declared here, in Editor.Ui, rather than
/// in Ui.Shell, specifically to break a circular dependency: Editor.Ui
/// needs to open tabs (via EditorOpeningService), and Ui.Shell needs to
/// open files from Project View (via a tool window factory that uses
/// Editor.Ui) — if EditorOpeningService depended on the concrete
/// Ui.Shell.EditorTabsHost class, the two assemblies would reference each
/// other directly. Depending on this interface instead means Ui.Shell
/// depends on Editor.Ui (to implement/reference this contract and to use
/// EditorOpeningService), while Editor.Ui has no reference back to Ui.Shell at all.
/// </summary>
public interface IEditorTabsHost
{
    /// <summary>
    /// Opens a new tab, or activates an existing one if a tab with the
    /// same ID is already open.
    /// </summary>
    /// <param name="id">
    /// A stable identifier for this tab.
    /// </param>
    /// <param name="title">
    /// The text shown in the tab's header.
    /// </param>
    /// <param name="content">
    /// The control to display as this tab's content.
    /// </param>
    void OpenTab(string id, string title, object content);

    /// <summary>
    /// Closes a tab by its ID, if currently open.
    /// </summary>
    /// <param name="id">
    /// The identifier of the tab to close.
    /// </param>
    void CloseTab(string id);
}