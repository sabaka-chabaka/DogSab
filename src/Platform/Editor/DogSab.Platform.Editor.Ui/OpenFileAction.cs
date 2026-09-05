using DogSab.Platform.Extensibility.Abstractions.Attributes;
using DogSab.Platform.Ui.Actions;
using DogSab.Platform.Ui.Actions.Abstractions;
using DogSab.Platform.Vfs.FileSystem;

namespace DogSab.Platform.Editor.Ui;

/// <summary>
/// The platform's "Open File" action: prompts... in a full implementation,
/// this would show a file picker dialog. For now, it demonstrates the
/// wiring by opening a fixed, hardcoded path — a real file picker is a
/// separate, not-yet-built piece of UI (a dialog querying the active
/// project's <see cref="Vfs.Abstractions.FileSystem.IVirtualFileSystem"/>
/// for a path), intentionally out of scope here since this action's
/// purpose is to prove the open-file pipeline end to end, not to build a
/// file picker.
/// </summary>
[Extension("ui.action")]
[MenuPlacement("File")]
public sealed class OpenFileAction : AnAction
{
    private readonly EditorOpeningService _openingService;
    private readonly VirtualFileSystemRouter _vfsRouter;

    /// <summary>
    /// Creates a new "Open File" action.
    /// </summary>
    /// <param name="openingService">
    /// The service used to actually open a resolved file for editing.
    /// </param>
    /// <param name="vfsRouter">
    /// Used to resolve a file path into an <see cref="Vfs.Abstractions.VirtualFile.IVirtualFile"/>.
    /// </param>
    public OpenFileAction(EditorOpeningService openingService, VirtualFileSystemRouter vfsRouter)
        : base("Open File...", "Opens a file for editing.")
    {
        _openingService = openingService;
        _vfsRouter = vfsRouter;
    }

    /// <inheritdoc />
    public override void Execute(ActionContext context)
    {
        // NOTE: hardcoded path — placeholder until a real file picker
        // dialog exists. Demonstrates the pipeline: resolve path -> VFS ->
        // EditorOpeningService -> EditorView -> EditorTabsHost.
        var file = _vfsRouter.FindFile("file:///placeholder-path.txt");

        if (file is not null)
        {
            _openingService.OpenFile(file);
        }
    }
}