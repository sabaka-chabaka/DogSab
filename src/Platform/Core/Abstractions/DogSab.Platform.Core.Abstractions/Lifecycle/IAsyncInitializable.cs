namespace DogSab.Platform.Core.Abstractions.Lifecycle;

public interface IAsyncInitializable
{
    Task InitializeAsync(CancellationToken cancellationToken);
}