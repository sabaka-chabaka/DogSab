using Avalonia.Controls;
using DogSab.Platform.Editor.Ui;

namespace DogSab.Platform.Ui.Shell;

/// <summary>
/// Hosts multiple open editor tabs within the main window's central content
/// area, replacing the single-content model that
/// <see cref="MainWindow.SetCentralContent"/> alone could not support.
/// Each tab's header is the file's display name, and its content is
/// whatever control the caller supplies (in practice, an
/// <c>Editor.Ui.EditorView</c> instance, though this type has no direct
/// dependency on the Editor module — it only knows about generic Avalonia
/// controls, keeping <c>Ui.Shell</c> the same kind of consumer of opaque
/// content it already is for tool windows).
/// </summary>
public partial class EditorTabsHost : UserControl, IEditorTabsHost
{
    /// <summary>
    /// Currently open tabs, keyed by a stable identifier (e.g. the file's
    /// virtual path), so re-opening an already-open file can find and
    /// activate its existing tab instead of opening a duplicate.
    /// </summary>
    private readonly Dictionary<string, TabItem> _tabsById = new();

    public EditorTabsHost()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Opens a new tab, or activates an existing one if a tab with the same
    /// ID is already open.
    /// </summary>
    /// <param name="id">
    /// A stable identifier for this tab, used to detect and activate an
    /// already-open tab instead of duplicating it.
    /// </param>
    /// <param name="title">
    /// The text shown in the tab's header.
    /// </param>
    /// <param name="content">
    /// The control to display as this tab's content.
    /// </param>
    public void OpenTab(string id, string title, object content)
    {
        if (_tabsById.TryGetValue(id, out var existingTab))
        {
            Tabs.SelectedItem = existingTab;
            return;
        }

        var tabItem = new TabItem
        {
            Header = title,
            Content = content
        };

        _tabsById[id] = tabItem;
        Tabs.Items.Add(tabItem);
        Tabs.SelectedItem = tabItem;
    }

    /// <summary>
    /// Closes a tab by its ID, if currently open.
    /// </summary>
    /// <param name="id">
    /// The identifier of the tab to close.
    /// </param>
    public void CloseTab(string id)
    {
        if (_tabsById.TryGetValue(id, out var tabItem))
        {
            Tabs.Items.Remove(tabItem);
            _tabsById.Remove(id);
        }
    }
}