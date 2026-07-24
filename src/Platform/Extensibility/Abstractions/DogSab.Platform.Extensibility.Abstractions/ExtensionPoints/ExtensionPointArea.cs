namespace DogSab.Platform.Extensibility.Abstractions.ExtensionPoints;

/// <summary>
/// Determines at which level an extension point's registered implementations
/// are shared, mirroring <see cref="Core.Abstractions.Services.ServiceScope"/>:
/// an application-area extension point has one shared set of registered
/// extensions for the whole process, while a project-area extension point's
/// registrations can differ per open project (e.g. a per-project VCS root
/// detector that only makes sense once a project's directory structure is known).
/// </summary>
public enum ExtensionPointArea
{
    /// <summary>Extensions are registered once for the entire application process.</summary>
    Application,

    /// <summary>Extensions may be registered per open project.</summary>
    Project
}