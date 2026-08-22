namespace DogSab.Platform.Ui.ToolWindows.Abstractions;

/// <summary>
/// A single dockable panel within the main window (e.g. Project View, a
/// future Debug panel). Implemented by platform subsystems or plugins;
/// registered via an <see cref="IToolWindowFactory"/> under
/// <see cref="ToolWindowExtensionPoints.TOOL_WINDOW"/>, not implemented directly
/// against Avalonia controls here — this interface deliberately says nothing
/// about the concrete UI technology, so a tool window's content is supplied
/// as an opaque object that the Shell module (which does know about
/// Avalonia) is responsible for hosting.
/// </summary>
public interface IToolWindow
{
    /// <summary>A stable identifier for this tool window, used for persistence (remembering which tool windows were open/docked where).</summary>
    string Id { get; }

    /// <summary>The display title shown in the tool window's tab/header.</summary>
    string Title { get; }

    /// <summary>Where this tool window docks by default.</summary>
    ToolWindowAnchor DefaultAnchor { get; }

    /// <summary>
    /// The tool window's content, as an opaque object. In practice this is
    /// expected to be an Avalonia <c>Control</c>, but the type is left as
    /// <see cref="object"/> here so <c>Ui.ToolWindows.Abstractions</c> itself
    /// has no dependency on Avalonia — only <c>Ui.Shell</c>, which hosts tool
    /// windows, needs to know how to cast and display it.
    /// </summary>
    object Content { get; }
}