using System.Collections.Concurrent;
using DogSab.Platform.Extensibility.Abstractions.ExtensionPoints;
using DogSab.Platform.Ui.ToolWindows.Abstractions;

namespace DogSab.Platform.Ui.ToolWindows;

/// <summary>
/// Manages the lifecycle of tool windows: tracks which
/// <see cref="IToolWindowFactory"/> instances are registered (via
/// <see cref="ToolWindowExtensionPoints.TOOL_WINDOW"/>) and which have
/// actually been instantiated as open <see cref="IToolWindow"/> instances.
/// Factories are cheap and registered eagerly at plugin load time; the
/// (potentially expensive) tool window instance itself is only created the
/// first time the user opens it, via <see cref="Open"/> — matching the
/// rationale already documented on <see cref="IToolWindowFactory"/>.
/// </summary>
public sealed class ToolWindowManagerImpl
{
    private readonly IExtensionPointRegistry _extensionPointRegistry;
    private readonly ConcurrentDictionary<string, IToolWindow> _openToolWindowsById = new();

    /// <summary>Raised when a tool window is opened, so <c>Ui.Shell</c>'s docking layout can host its content.</summary>
    public event Action<IToolWindow>? ToolWindowOpened;

    /// <summary>Raised when a tool window is closed, so <c>Ui.Shell</c> can remove it from the docking layout.</summary>
    public event Action<string>? ToolWindowClosed;

    public ToolWindowManagerImpl(IExtensionPointRegistry extensionPointRegistry)
    {
        _extensionPointRegistry = extensionPointRegistry;
    }

    /// <summary>Every registered factory, usable to build a "View → Tool Windows" menu listing every available (not necessarily open) tool window.</summary>
    public IReadOnlyList<IToolWindowFactory> AllFactories =>
        _extensionPointRegistry.GetExtensions(ToolWindowExtensionPoints.TOOL_WINDOW);

    /// <summary>Every currently open tool window instance.</summary>
    public IReadOnlyList<IToolWindow> OpenToolWindows => new List<IToolWindow>(_openToolWindowsById.Values);

    /// <summary>
    /// Opens a tool window by its ID, creating it via its registered factory
    /// if not already open. If already open, this is a no-op — reopening an
    /// already-open tool window is expected to just bring it to focus, which
    /// is a <c>Ui.Shell</c> presentation concern, not something this manager tracks.
    /// </summary>
    /// <param name="toolWindowId">The ID of the tool window to open, matching some registered <see cref="IToolWindowFactory.ToolWindowId"/>.</param>
    /// <returns>The now-open tool window instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no factory is registered for this ID.</exception>
    public IToolWindow Open(string toolWindowId)
    {
        if (_openToolWindowsById.TryGetValue(toolWindowId, out var existing))
        {
            return existing;
        }

        var factory = FindFactory(toolWindowId)
            ?? throw new InvalidOperationException($"No tool window factory registered for id '{toolWindowId}'.");

        var toolWindow = factory.Create();
        _openToolWindowsById[toolWindowId] = toolWindow;

        ToolWindowOpened?.Invoke(toolWindow);

        return toolWindow;
    }

    /// <summary>
    /// Closes an open tool window by ID. No-op if it isn't currently open.
    /// </summary>
    /// <param name="toolWindowId">The ID of the tool window to close.</param>
    public void Close(string toolWindowId)
    {
        if (_openToolWindowsById.TryRemove(toolWindowId, out _))
        {
            ToolWindowClosed?.Invoke(toolWindowId);
        }
    }

    /// <summary>
    /// Finds the registered factory for a given tool window ID.
    /// </summary>
    /// <param name="toolWindowId">The tool window ID to look up.</param>
    /// <returns>The matching factory, or <c>null</c> if none is registered under this ID.</returns>
    private IToolWindowFactory? FindFactory(string toolWindowId)
    {
        foreach (var factory in AllFactories)
        {
            if (factory.ToolWindowId == toolWindowId)
            {
                return factory;
            }
        }

        return null;
    }
}