namespace DogSab.Platform.Core.Abstractions.Lifecycle;

/// <summary>Implemented by types that require an explicit asynchronous initialization step.</summary>
public interface IAsyncInitializable
{
    /// <summary>
    /// Performs asynchronous initialization logic. Called once by the owning container.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel initialization, e.g. on shutdown.</param>
    /// <returns>A task that completes when initialization has finished.</returns>
    Task InitializeAsync(CancellationToken cancellationToken);
}