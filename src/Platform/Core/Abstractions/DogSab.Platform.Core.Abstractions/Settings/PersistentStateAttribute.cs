namespace DogSab.Platform.Core.Abstractions.Settings;

/// <summary>Marks a class as persistent state managed automatically by the platform's settings system.</summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class PersistentStateAttribute : Attribute
{
    /// <summary>The file name (without directory) used to store this state.</summary>
    public string FileName { get; }

    /// <summary>The scope at which this state is stored. Defaults to <see cref="SettingsScope.Project"/>.</summary>
    public SettingsScope Scope { get; init; } = SettingsScope.Project;

    /// <summary>
    /// Creates a new persistent-state marker.
    /// </summary>
    /// <param name="fileName">The file name (without directory) used to store this state.</param>
    public PersistentStateAttribute(string fileName)
    {
        FileName = fileName;
    }
}