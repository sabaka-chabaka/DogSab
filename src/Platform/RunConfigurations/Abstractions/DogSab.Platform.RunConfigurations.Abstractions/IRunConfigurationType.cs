using DogSab.Platform.ProjectModel.Abstractions.Module;

namespace DogSab.Platform.RunConfigurations.Abstractions;

/// <summary>
/// Provides a kind of run configuration and knows how to actually derive a
/// launch command for it — implemented per toolchain/language (e.g. a
/// future <c>DotnetRunConfigurationType</c> plugin knows to launch
/// <c>dotnet run --project &lt;path&gt;</c> for a .NET module) and
/// registered against <see cref="Events.RunExtensionPoints.RUN_CONFIGURATION_TYPE"/>.
/// The platform itself never constructs an actual OS process command line
/// directly — that knowledge belongs entirely to whichever
/// <see cref="IRunConfigurationType"/> a configuration was created from.
/// </summary>
public interface IRunConfigurationType
{
    /// <summary>
    /// A stable identifier for this configuration type (e.g.
    /// <c>"dotnet.run"</c>), stored alongside a configuration so the
    /// platform knows which type to ask for a launch specification when
    /// the configuration is later run.
    /// </summary>
    string TypeId { get; }

    /// <summary>
    /// A human-readable display name, shown when the user creates a new
    /// run configuration and picks its type (e.g. <c>".NET Run"</c>).
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Determines whether this configuration type can meaningfully target
    /// the given module — e.g. a .NET run configuration type only applies
    /// to modules whose content roots contain a recognizable .NET project,
    /// not to a module written in an unrelated language.
    /// </summary>
    /// <param name="moduleId">
    /// The module to check applicability for.
    /// </param>
    /// <returns>
    /// <c>true</c> if this type can create a configuration targeting this
    /// module; otherwise <c>false</c>.
    /// </returns>
    bool CanTarget(ModuleId moduleId);

    /// <summary>
    /// Derives the actual OS-level launch specification for a
    /// configuration of this type — the executable path and base arguments
    /// needed to start the process, before the configuration's own
    /// <see cref="IRunConfiguration.Arguments"/> are appended.
    /// </summary>
    /// <param name="configuration">
    /// The configuration to derive a launch specification for.
    /// </param>
    /// <returns>
    /// The resolved launch specification.
    /// </returns>
    LaunchSpecification CreateLaunchSpecification(IRunConfiguration configuration);
}

/// <summary>
/// The concrete, OS-level information needed to actually start a process:
/// the executable to run and the base arguments to pass it, before a
/// configuration's own <see cref="IRunConfiguration.Arguments"/> and
/// <see cref="IRunConfiguration.EnvironmentVariables"/> are layered on top
/// by <see cref="ProcessRunner"/>.
/// </summary>
public readonly struct LaunchSpecification
{
    /// <summary>
    /// The path to the executable to launch (e.g. <c>"dotnet"</c>).
    /// </summary>
    public string ExecutablePath { get; }

    /// <summary>
    /// Base arguments derived by the configuration type itself (e.g.
    /// <c>["run", "--project", "MyApp.Api.csproj"]</c>), before the
    /// configuration's own user-supplied <see cref="IRunConfiguration.Arguments"/>
    /// are appended.
    /// </summary>
    public string[] BaseArguments { get; }

    /// <summary>
    /// The working directory the process should be started in.
    /// </summary>
    public string WorkingDirectory { get; }

    /// <summary>
    /// Creates a new launch specification.
    /// </summary>
    /// <param name="executablePath">
    /// The path to the executable to launch.
    /// </param>
    /// <param name="baseArguments">
    /// The base arguments derived by the configuration type.
    /// </param>
    /// <param name="workingDirectory">
    /// The working directory to launch the process in.
    /// </param>
    public LaunchSpecification(string executablePath, string[] baseArguments, string workingDirectory)
    {
        ExecutablePath = executablePath;
        BaseArguments = baseArguments;
        WorkingDirectory = workingDirectory;
    }
}