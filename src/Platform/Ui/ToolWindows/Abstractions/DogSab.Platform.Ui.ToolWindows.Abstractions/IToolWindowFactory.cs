namespace DogSab.Platform.Ui.ToolWindows.Abstractions;

/// <summary>
/// Creates <see cref="IToolWindow"/> instances on demand. Registered against
/// <see cref="ToolWindowExtensionPoints.TOOL_WINDOW"/> rather than
/// registering a pre-built <see cref="IToolWindow"/> directly, so tool
/// windows are only actually constructed (and their potentially expensive
/// content built) when the user first opens them, not eagerly at plugin load time.
/// </summary>
public interface IToolWindowFactory
{
    /// <summary>A stable identifier matching the tool window this factory creates, used before construction (e.g. to build a menu entry) without needing to instantiate it.</summary>
    string ToolWindowId { get; }

    /// <summary>The display title, available before construction for the same reason as <see cref="ToolWindowId"/>.</summary>
    string Title { get; }

    /// <summary>
    /// Creates a new tool window instance.
    /// </summary>
    /// <returns>The newly created tool window.</returns>
    IToolWindow Create();
}