using DogSab.Platform.Core.Abstractions.Settings;

namespace DogSab.Platform.Core.Settings.Impl.Paths;

/// <summary>
/// Resolves the on-disk directory and file path used to persist a settings
/// object, based on its declared <see cref="SettingsScope"/>. Application-scope
/// settings live in a global per-user directory; project- and workspace-scope
/// settings live inside the currently open project's own <c>.dogsab</c> folder.
/// </summary>
public sealed class SettingsPathResolver
{
    /// <summary>The root directory of the currently open project, or <c>null</c> if none is open.</summary>
    private readonly string? _projectRootDirectory;
    
    /// <summary>
    /// Creates a new path resolver.
    /// </summary>
    /// <param name="projectRootDirectory">
    /// The root directory of the currently open project, used to resolve
    /// <see cref="SettingsScope.Project"/> and <see cref="SettingsScope.Workspace"/> paths.
    /// Pass <c>null</c> if no project is open (only <see cref="SettingsScope.Application"/> settings can then be resolved).
    /// </param>
    public SettingsPathResolver(string? projectRootDirectory)
    {
        _projectRootDirectory = projectRootDirectory;
    }
    
    /// <summary>
    /// Resolves the full file path for a settings file with the given name and scope,
    /// creating the containing directory if it does not already exist.
    /// </summary>
    /// <param name="fileName">The settings file name, as declared via <see cref="PersistentStateAttribute.FileName"/>.</param>
    /// <param name="scope">The scope the settings belong to.</param>
    /// <returns>The absolute path to the settings file.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if <paramref name="scope"/> is <see cref="SettingsScope.Project"/> or
    /// <see cref="SettingsScope.Workspace"/> but no project is currently open.
    /// </exception>
    public string ResolveFilePath(string fileName, SettingsScope scope)
    {
        var directory = ResolveDirectory(scope);
        Directory.CreateDirectory(directory);
        return Path.Combine(directory, fileName);
    }

    /// <summary>
    /// Resolves the directory for a given settings scope.
    /// </summary>
    /// <param name="scope">The scope to resolve a directory for.</param>
    /// <returns>The absolute directory path for that scope.</returns>
    private string ResolveDirectory(SettingsScope scope)
    {
        return scope switch
        {
            SettingsScope.Application => GetApplicationSettingsDirectory(),
            SettingsScope.Project => RequireProjectDirectory("project"),
            SettingsScope.Workspace => RequireProjectDirectory("workspace"),
            _ => throw new InvalidOperationException($"Unrecognized settings scope '{scope}'.")
        };
    }

    /// <summary>
    /// Returns the global, per-user directory for application-scope settings,
    /// following each OS's conventional location for application configuration.
    /// </summary>
    /// <returns>The absolute path to the application settings directory.</returns>
    private static string GetApplicationSettingsDirectory()
    {
        string baseDirectory;

        if (OperatingSystem.IsWindows())
        {
            baseDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        }
        else if (OperatingSystem.IsMacOS())
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            baseDirectory = Path.Combine(home, "Library", "Application Support");
        }
        else
        {
            var xdgConfigHome = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
            baseDirectory = !string.IsNullOrEmpty(xdgConfigHome)
                ? xdgConfigHome
                : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config");
        }

        return Path.Combine(baseDirectory, "DogSab", "config");
    }

    /// <summary>
    /// Returns the project-relative <c>.dogsab</c> settings directory, requiring
    /// that a project is currently open.
    /// </summary>
    /// <param name="subFolderName">The subfolder name under <c>.dogsab</c> (e.g. "project" or "workspace").</param>
    /// <returns>The absolute path to the project-scoped settings directory.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no project is currently open.</exception>
    private string RequireProjectDirectory(string subFolderName)
    {
        if (_projectRootDirectory is null)
        {
            throw new InvalidOperationException(
                $"Cannot resolve a '{subFolderName}'-scoped settings path: no project is currently open.");
        }

        return Path.Combine(_projectRootDirectory, ".dogsab", subFolderName);
    }
}