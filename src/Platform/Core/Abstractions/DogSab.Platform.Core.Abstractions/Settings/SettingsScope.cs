namespace DogSab.Platform.Core.Abstractions.Settings;

/// <summary>Defines at which level a settings object is stored.</summary>
public enum SettingsScope
{
    /// <summary>Global settings for the whole application instance (~/.dogsab/config).</summary>
    Application,

    /// <summary>Project settings, usually committed to VCS (.dogsab/*.xml).</summary>
    Project,

    /// <summary>Local working-copy settings, not committed (.dogsab/workspace.xml).</summary>
    Workspace
}