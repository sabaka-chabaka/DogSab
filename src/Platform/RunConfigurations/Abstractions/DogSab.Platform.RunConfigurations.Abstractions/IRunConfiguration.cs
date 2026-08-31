using DogSab.Platform.ProjectModel.Abstractions.Module;

namespace DogSab.Platform.RunConfigurations.Abstractions;

/// <summary>
/// A single user-configured way to run something — e.g. "Run MyApp.Api with
/// environment ASPNETCORE_ENVIRONMENT=Development" or "Run all tests in
/// MyApp.Tests".
/// The platform itself has no built-in notion of how to actually launch a
/// process for a given module (that is language/toolchain-specific — a
/// .NET module launches via <c>dotnet run</c>, a Node module via
/// <c>node</c>), so this interface only describes the configuration data
/// itself; the actual launch command is derived by whichever
/// <see cref="IRunConfigurationType"/> created this configuration, when
/// <see cref="ProcessRunner"/> asks it to build a launch specification.
/// </summary>
public interface IRunConfiguration
{
    /// <summary>
    /// This configuration's stable identifier, persisted across sessions
    /// so the same configuration can be re-selected and re-run later.
    /// </summary>
    RunConfigurationId Id { get; }

    /// <summary>
    /// A human-readable display name, shown in the run configuration
    /// dropdown (e.g. <c>"MyApp.Api"</c>, <c>"MyApp.Api (Debug)"</c>).
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// The module this configuration launches. Used, among other things,
    /// to resolve the module's content roots as the working directory a
    /// launched process should start in.
    /// </summary>
    ModuleId TargetModuleId { get; }

    /// <summary>
    /// Command-line arguments to pass to the launched process, in addition
    /// to whatever base command the owning <see cref="IRunConfigurationType"/>
    /// derives for this configuration's target.
    /// </summary>
    string[] Arguments { get; }

    /// <summary>
    /// Environment variables to set for the launched process, in addition
    /// to those inherited from the platform's own process environment.
    /// </summary>
    System.Collections.Generic.IReadOnlyDictionary<string, string> EnvironmentVariables { get; }
}