using Avalonia;
using Avalonia.Markup.Xaml;

namespace DogSab.Platform.Host;

/// <summary>
/// The Avalonia application object. Deliberately does not itself create or
/// show the main window — that already happens inside
/// <c>Ui.Shell.Startup.UiShellStartupActivity</c>, which runs as part of
/// <see cref="Core.Application.EntryPoints.ApplicationBuilder.Start"/>'s
/// startup activity pipeline, before Avalonia's own lifetime even begins.
/// This class's role is purely to satisfy Avalonia's own required
/// application bootstrap shape.
/// </summary>
public partial class App : Application
{
    /// <inheritdoc />
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    /// <inheritdoc />
    public override void OnFrameworkInitializationCompleted()
    {
        base.OnFrameworkInitializationCompleted();
    }
}