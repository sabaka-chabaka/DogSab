using DogSab.Platform.Ui.ToolWindows;
using DogSab.Platform.Ui.ToolWindows.Abstractions;

namespace DogSab.Platform.Ui.Shell.Docking;

/// <summary>
/// Bridges <see cref="ToolWindowManagerImpl"/>'s open tool windows into
/// <see cref="DockLayoutManager"/>'s docked panels. Lives in <c>Ui.Shell</c>
/// specifically because it is the one place in the platform allowed to
/// depend on both the ToolWindows and Docking modules — neither of those
/// modules is allowed to depend on the other, or on each other's specific
/// concepts (<see cref="ToolWindowAnchor"/> vs <see cref="DockEdge"/>), so
/// the conversion between them happens here instead.
/// </summary>
public sealed class ToolWindowDockingBridge
{
    private readonly ToolWindowManagerImpl _toolWindowManager;
    private readonly DockLayoutManager _dockLayoutManager;

    /// <summary>
    /// Creates a new bridge and subscribes to tool window open/close events
    /// for the rest of its lifetime.
    /// </summary>
    /// <param name="toolWindowManager">The tool window manager to observe.</param>
    /// <param name="dockLayoutManager">The dock layout manager to update.</param>
    public ToolWindowDockingBridge(ToolWindowManagerImpl toolWindowManager, DockLayoutManager dockLayoutManager)
    {
        _toolWindowManager = toolWindowManager;
        _dockLayoutManager = dockLayoutManager;

        _toolWindowManager.ToolWindowOpened += OnToolWindowOpened;
        _toolWindowManager.ToolWindowClosed += OnToolWindowClosed;
    }

    /// <summary>
    /// Docks a newly opened tool window at the layout edge corresponding to
    /// its <see cref="IToolWindow.DefaultAnchor"/>, wrapping it as an
    /// <see cref="IDockPanel"/> via <see cref="ToolWindowDockPanelAdapter"/>.
    /// </summary>
    /// <param name="toolWindow">The tool window that was just opened.</param>
    private void OnToolWindowOpened(IToolWindow toolWindow)
    {
        var edge = ToAnchorEdge(toolWindow.DefaultAnchor);
        var panel = new ToolWindowDockPanelAdapter(toolWindow);

        _dockLayoutManager.Dock(panel, edge);
    }

    /// <summary>Undocks a closed tool window's panel by its ID.</summary>
    /// <param name="toolWindowId">The ID of the tool window that was just closed.</param>
    private void OnToolWindowClosed(string toolWindowId)
    {
        _dockLayoutManager.Undock(toolWindowId);
    }

    /// <summary>
    /// Converts a <see cref="ToolWindowAnchor"/> to its equivalent <see cref="DockEdge"/>.
    /// The one-to-one mapping this module exists to perform.
    /// </summary>
    /// <param name="anchor">The tool window anchor to convert.</param>
    /// <returns>The corresponding dock edge.</returns>
    private static DockEdge ToAnchorEdge(ToolWindowAnchor anchor) => anchor switch
    {
        ToolWindowAnchor.Left => DockEdge.Left,
        ToolWindowAnchor.Right => DockEdge.Right,
        ToolWindowAnchor.Bottom => DockEdge.Bottom,
        _ => throw new System.ArgumentOutOfRangeException(nameof(anchor), anchor, "Unrecognized tool window anchor.")
    };
}

/// <summary>Adapts an <see cref="IToolWindow"/> to the <see cref="IDockPanel"/> contract, so it can be tracked by <see cref="DockLayoutManager"/> without that module needing to know about tool windows directly.</summary>
internal sealed class ToolWindowDockPanelAdapter : IDockPanel
{
    private readonly IToolWindow _toolWindow;

    public ToolWindowDockPanelAdapter(IToolWindow toolWindow)
    {
        _toolWindow = toolWindow;
    }

    public string Id => _toolWindow.Id;
    public string Title => _toolWindow.Title;
    public object Content => _toolWindow.Content;
}