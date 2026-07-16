namespace DogSab.Platform.Core.Abstractions.Services;

/// <summary>Defines at which level a service instance is shared.</summary>
public enum ServiceScope
{
    /// <summary>One shared instance for the entire application process.</summary>
    Application,

    /// <summary>One instance per open project.</summary>
    Project
}