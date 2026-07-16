namespace DogSab.Platform.Core.Abstractions.Lifecycle;

/// <summary>
/// An action executed after application startup or project opening.
/// Registered by plugins via an Extension Point, executed by the platform
/// on a background thread without blocking the UI.
/// </summary>
public interface IStartupActivity
{
    /// <summary>
    /// Runs the startup logic contributed by a plugin.
    /// </summary>
    /// <param name="cancellationToken">Token signaled if startup is aborted (e.g. project closed early).</param>
    /// <returns>A task that completes when the activity has finished running.</returns>
    Task RunActivityAsync(CancellationToken cancellationToken);

    /// <summary>Execution order relative to other activities (lower values run first). Defaults to 0.</summary>
    int Order => 0;
}