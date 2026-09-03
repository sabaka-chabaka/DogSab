using Avalonia.Controls;
using DogSab.Platform.Extensibility.Abstractions.Loading;

namespace DogSab.Platform.PluginManager.Ui;

/// <summary>
/// A single row in the plugin manager's list, showing a plugin's name,
/// version, current <see cref="PluginLoadState"/>, and an action button
/// whose label and behavior adapt to that state — "Uninstall" for a
/// successfully loaded plugin, "Remove" for a failed one, since offering
/// to "uninstall" something that never actually loaded would be a
/// confusing label for what is really just cleaning up a broken entry.
/// </summary>
public partial class PluginListItemView : UserControl
{
    /// <summary>
    /// Raised when the user clicks this row's action button, carrying the
    /// descriptor of the plugin the action applies to.
    /// </summary>
    public event Action<IPluginDescriptor>? ActionRequested;

    /// <summary>
    /// The descriptor this row currently displays.
    /// </summary>
    private IPluginDescriptor? _descriptor;

    public PluginListItemView()
    {
        InitializeComponent();
        ActionButton.Click += (_, _) =>
        {
            if (_descriptor is not null)
            {
                ActionRequested?.Invoke(_descriptor);
            }
        };
    }

    /// <summary>
    /// Populates this row with a plugin descriptor's current information.
    /// </summary>
    /// <param name="descriptor">
    /// The plugin descriptor to display.
    /// </param>
    public void SetDescriptor(IPluginDescriptor descriptor)
    {
        _descriptor = descriptor;

        NameText.Text = descriptor.Manifest.DisplayName;
        VersionText.Text = descriptor.Manifest.Version.ToString();
        StatusText.Text = descriptor.State.ToString();

        ActionButton.Content = descriptor.State switch
        {
            PluginLoadState.Failed => "Remove",
            _ => "Uninstall"
        };
    }
}