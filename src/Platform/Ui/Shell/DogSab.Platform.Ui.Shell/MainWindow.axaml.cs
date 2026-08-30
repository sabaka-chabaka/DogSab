using Avalonia.Controls;

namespace DogSab.Platform.Ui.Shell;

/// <summary>
/// The platform's root application window: hosts the menu bar (top), status
/// bar (bottom), and central content area, progressively filled in with
/// docked tool windows and, eventually, the editor area.
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    /// <summary>Sets the menu bar control.</summary>
    /// <param name="menu">The menu control to host.</param>
    public void SetMenuBar(Menu menu)
    {
        MenuBarHost.Content = menu;
    }

    /// <summary>Sets the status bar control.</summary>
    /// <param name="statusBar">The status bar control to host.</param>
    public void SetStatusBar(StatusBar statusBar)
    {
        StatusBarHost.Content = statusBar;
    }

    /// <summary>Sets the central content area.</summary>
    /// <param name="content">The content to display.</param>
    public void SetCentralContent(object content)
    {
        CentralContentHost.Content = content;
    }
    
    /// <summary>
    /// Sets the central content area to host editor tabs.
    /// Replaces the earlier single-<see cref="ContentControl.Content"/>
    /// approach, which had no way to represent multiple simultaneously open
    /// files — see <see cref="EditorTabsHost"/> for the tab management logic itself.
    /// </summary>
    /// <param name="editorTabsHost">
    /// The tabs host to display in the central content area.
    /// </param>
    public void SetEditorTabsHost(EditorTabsHost editorTabsHost)
    {
        CentralContentHost.Content = editorTabsHost;
    }
}