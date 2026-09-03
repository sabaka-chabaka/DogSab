using Avalonia.Controls;
using Avalonia.Platform.Storage;
using DogSab.Platform.Extensibility.Abstractions.Loading;
using DogSab.Platform.PluginManager.Installation;

namespace DogSab.Platform.PluginManager.Ui;

/// <summary>
/// The platform's plugin management window: lists every discovered plugin
/// (loaded, failed, or disabled), shows details for the selected one, and
/// lets the user install a new plugin from a local <c>.zip</c> archive or
/// uninstall an existing one.
/// Uninstalling and newly installing a plugin both take effect only after
/// a restart in this implementation — this dialog does not attempt to hot-load
/// or hot-unload a plugin into the currently running session's UI beyond
/// what <see cref="Installation.PluginUninstaller"/> already does for
/// in-memory unloading; it does not, for example, retroactively remove an
/// already-rendered tool window a just-uninstalled plugin contributed.
/// </summary>
public partial class PluginManagerDialog : Window
{
    /// <summary>
    /// The platform's plugin loader, used to re-discover and re-list
    /// plugins after an install/uninstall operation.
    /// </summary>
    private IPluginLoader? _pluginLoader;

    /// <summary>
    /// Installs plugins from a local archive.
    /// </summary>
    private PluginInstaller? _installer;

    /// <summary>
    /// Uninstalls plugins.
    /// </summary>
    private PluginUninstaller? _uninstaller;

    /// <summary>
    /// The platform's plugins root directory, used for both discovery and
    /// new installations.
    /// </summary>
    private string? _pluginsRootDirectory;

    /// <summary>
    /// The currently displayed plugin descriptors.
    /// </summary>
    private IReadOnlyList<IPluginDescriptor> _currentDescriptors = new List<IPluginDescriptor>();

    public PluginManagerDialog()
    {
        InitializeComponent();
        InstallFromFileButton.Click += OnInstallFromFileClicked;
    }

    /// <summary>
    /// Wires up this dialog's dependencies — supplied externally rather
    /// than resolved internally, since this dialog has no access to the DI
    /// container itself and is expected to be constructed by whatever code
    /// opens it (see <see cref="Startup.PluginManagerAction"/>), which does
    /// have that access.
    /// </summary>
    /// <param name="pluginLoader">
    /// The platform's plugin loader.
    /// </param>
    /// <param name="installer">
    /// Used to install plugins from a local archive.
    /// </param>
    /// <param name="uninstaller">
    /// Used to uninstall plugins.
    /// </param>
    /// <param name="pluginsRootDirectory">
    /// The platform's plugins root directory.
    /// </param>
    public async void Configure(
        IPluginLoader pluginLoader,
        PluginInstaller installer,
        PluginUninstaller uninstaller,
        string pluginsRootDirectory)
    {
        _pluginLoader = pluginLoader;
        _installer = installer;
        _uninstaller = uninstaller;
        _pluginsRootDirectory = pluginsRootDirectory;

        await RefreshListAsync();
    }

    /// <summary>
    /// Re-discovers every plugin in the plugins directory and refreshes
    /// the displayed list.
    /// </summary>
    private async System.Threading.Tasks.Task RefreshListAsync()
    {
        if (_pluginLoader is null || _pluginsRootDirectory is null)
        {
            return;
        }

        _currentDescriptors = await _pluginLoader.DiscoverAsync(_pluginsRootDirectory, CancellationToken.None);

        var rows = _currentDescriptors.Select(BuildRow).ToList();
        PluginsList.ItemsSource = rows;

        DetailsHost.Content = null;
    }

    /// <summary>
    /// Builds a single list row for a plugin descriptor, wiring its
    /// selection and action events.
    /// </summary>
    /// <param name="descriptor">
    /// The descriptor to build a row for.
    /// </param>
    /// <returns>
    /// The built row control.
    /// </returns>
    private PluginListItemView BuildRow(IPluginDescriptor descriptor)
    {
        var row = new PluginListItemView();
        row.SetDescriptor(descriptor);

        row.PointerPressed += (_, _) =>
        {
            var detailsPanel = new PluginDetailsPanel();
            detailsPanel.SetDescriptor(descriptor);
            DetailsHost.Content = detailsPanel;
        };

        row.ActionRequested += OnRowActionRequested;

        return row;
    }

    /// <summary>
    /// Handles the action button click on a plugin row: uninstalls the
    /// plugin and refreshes the list.
    /// </summary>
    /// <param name="descriptor">
    /// The descriptor the action applies to.
    /// </param>
    private async void OnRowActionRequested(IPluginDescriptor descriptor)
    {
        if (_uninstaller is null)
        {
            return;
        }

        _uninstaller.Uninstall(descriptor.Manifest.Id, descriptor.PluginDirectory);

        await RefreshListAsync();
    }

    /// <summary>
    /// Opens a file picker for a <c>.zip</c> archive, installs it, and
    /// refreshes the list.
    /// </summary>
    private async void OnInstallFromFileClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_installer is null || _pluginsRootDirectory is null)
        {
            return;
        }

        var storageProvider = StorageProvider;
        var files = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select a plugin archive",
            AllowMultiple = false,
            FileTypeFilter = new[] { new FilePickerFileType("Plugin archive") { Patterns = new[] { "*.zip" } } }
        });

        var selectedFile = files.FirstOrDefault();
        if (selectedFile is null)
        {
            return;
        }

        _installer.Install(selectedFile.Path.LocalPath, _pluginsRootDirectory);

        await RefreshListAsync();
    }
}