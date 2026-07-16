namespace DogSab.Platform.Core.Abstractions.Lifecycle;

public interface IStartupActivity
{
    Task RunActivityAsync(CancellationToken cancellationToken);

    int Order => 0;
}