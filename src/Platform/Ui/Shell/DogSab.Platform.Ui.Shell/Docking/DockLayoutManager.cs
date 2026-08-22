namespace DogSab.Platform.Ui.Shell.Docking;

/// <summary>
/// Tracks which <see cref="IDockPanel"/> instances are docked at which edge
/// of the main window (left, right, bottom), independent of how those edges
/// are actually rendered — <see cref="MainWindow"/> observes this manager's
/// change events and is responsible for the actual Avalonia layout.
/// </summary>
public sealed class DockLayoutManager
{
    private readonly Dictionary<DockEdge, List<IDockPanel>> _panelsByEdge = new()
    {
        [DockEdge.Left] = new List<IDockPanel>(),
        [DockEdge.Right] = new List<IDockPanel>(),
        [DockEdge.Bottom] = new List<IDockPanel>()
    };

    /// <summary>Raised whenever the layout changes — a panel is docked, undocked, or moved to a different edge.</summary>
    public event Action? LayoutChanged;

    /// <summary>
    /// Docks a panel at the given edge, appending it after any panels
    /// already docked there. If the panel is already docked elsewhere, it is
    /// moved rather than duplicated.
    /// </summary>
    /// <param name="panel">The panel to dock.</param>
    /// <param name="edge">The edge to dock it at.</param>
    public void Dock(IDockPanel panel, DockEdge edge)
    {
        Undock(panel.Id);

        _panelsByEdge[edge].Add(panel);
        LayoutChanged?.Invoke();
    }

    /// <summary>
    /// Removes a panel from the layout entirely, by ID.
    /// </summary>
    /// <param name="panelId">The ID of the panel to remove.</param>
    public void Undock(string panelId)
    {
        var removedAny = false;

        foreach (var panels in _panelsByEdge.Values)
        {
            removedAny |= panels.RemoveAll(p => p.Id == panelId) > 0;
        }

        if (removedAny)
        {
            LayoutChanged?.Invoke();
        }
    }

    /// <summary>Returns the panels currently docked at a given edge, in display order.</summary>
    /// <param name="edge">The edge to query.</param>
    /// <returns>The panels docked there.</returns>
    public IReadOnlyList<IDockPanel> GetPanels(DockEdge edge) => _panelsByEdge[edge];
}

/// <summary>The edge of the main window a panel is docked to.</summary>
public enum DockEdge
{
    Left,
    Right,
    Bottom
}