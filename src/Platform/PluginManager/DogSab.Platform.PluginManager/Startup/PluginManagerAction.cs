using DogSab.Platform.Extensibility.Abstractions.Loading;
using DogSab.Platform.PluginManager.Installation;
using DogSab.Platform.PluginManager.Ui;
using DogSab.Platform.Ui.Actions;
using DogSab.Platform.Ui.Actions.Abstractions;

namespace DogSab.Platform.PluginManager.Startup;

/// <summary>
/// The platform's "Manage Plugins..." action: constructs and shows a
/// configured <see cref="PluginManagerDialog"/>.
/// Placed under the "File" menu grouping alongside <c>OpenFileAction</c>
/// (see <c>Editor.Ui</c>) for lack of a more specific settings/tools menu
/// grouping having been established yet in the platform's action taxonomy —
/// a real product would more likely place this under a dedicated
/// "Settings" or "Tools" top-level menu once one exists.
/// </summary>
[MenuPlacement("File")]
public sealed class PluginManagerAction : AnAction
{
    /// <summary>
    /// The platform's plugin loader, passed through to the opened dialog.
    /// </summary>
    private readonly IPluginLoader _pluginLoader;

    /// <summary>
    /// Used to install plugins from a local archive, passed through to the
    /// opened dialog.
    /// </summary>
    private readonly PluginInstaller _installer;

    /// <summary>
    /// Used to uninstall plugins, passed through to the opened dialog.
    /// </summary>
    private readonly PluginUninstaller _uninstaller;

    /// <summary>
    /// The platform's plugins root directory, passed through to the opened dialog.
    /// </summary>
    private readonly string _pluginsRootDirectory;

    /// <summary>
    /// Creates a new "Manage Plugins..." action.
    /// </summary>
    /// <param name="pluginLoader">
    /// The platform's plugin loader.
    /// </param>
    /// <param name="installer">
    /// The plugin installer to use.
    /// </param>
    /// <param name="uninstaller">
    /// The plugin uninstaller to use.
    /// </param>
    /// <param name="pluginsRootDirectory">
    /// The platform's plugins root directory.
    /// </param>
    public PluginManagerAction(
        IPluginLoader pluginLoader,
        PluginInstaller installer,
        PluginUninstaller uninstaller,
        string pluginsRootDirectory)
        : base("Manage Plugins...", "Opens the plugin manager to install, uninstall, or inspect plugins.")
    {
        _pluginLoader = pluginLoader;
        _installer = installer;
        _uninstaller = uninstaller;
        _pluginsRootDirectory = pluginsRootDirectory;
    }

    /// <inheritdoc />
    public override void Execute(ActionContext context)
    {
        var dialog = new PluginManagerDialog();
        dialog.Configure(_pluginLoader, _installer, _uninstaller, _pluginsRootDirectory);
        dialog.Show();
    }
}