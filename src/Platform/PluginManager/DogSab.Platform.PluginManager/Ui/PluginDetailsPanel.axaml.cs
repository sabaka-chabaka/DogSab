using DogSab.Platform.Extensibility.Abstractions.Loading;

namespace DogSab.Platform.PluginManager.Ui;

using Avalonia.Controls;

/// <summary>
/// Displays the full details of whichever plugin is currently selected in
/// <see cref="PluginManagerDialog"/>'s list — description, author,
/// dependencies, and (when applicable) load warnings or a failure reason.
/// </summary>
public partial class PluginDetailsPanel : UserControl
{
    public PluginDetailsPanel()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Populates the panel with a plugin descriptor's full details.
    /// </summary>
    /// <param name="descriptor">
    /// The plugin descriptor to display, or <c>null</c> to clear the panel
    /// when nothing is selected.
    /// </param>
    public void SetDescriptor(IPluginDescriptor? descriptor)
    {
        if (descriptor is null)
        {
            IsVisible = false;
            return;
        }

        IsVisible = true;

        var manifest = descriptor.Manifest;

        DisplayNameText.Text = manifest.DisplayName;
        AuthorText.Text = string.IsNullOrEmpty(manifest.Author) ? "Unknown author" : manifest.Author;
        VersionText.Text = $"Version {manifest.Version}";
        DescriptionText.Text = manifest.Description;

        DependenciesList.ItemsSource = manifest.Dependencies
            .Select(d => $"{d.DependencyPluginId} {d.AcceptableVersionRange}" + (d.IsOptional ? " (optional)" : string.Empty))
            .ToList();

        var hasFailure = descriptor.State == PluginLoadState.Failed && descriptor.FailureReason is not null;
        FailureReasonText.IsVisible = hasFailure;
        FailureReasonText.Text = hasFailure ? $"Failed to load: {descriptor.FailureReason}" : string.Empty;
    }
}