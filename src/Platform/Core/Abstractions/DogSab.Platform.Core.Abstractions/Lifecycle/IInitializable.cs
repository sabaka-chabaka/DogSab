namespace DogSab.Platform.Core.Abstractions.Lifecycle;

/// <summary>Implemented by types that require an explicit synchronous initialization step.</summary>
public interface IInitializable
{
    /// <summary>Performs synchronous initialization logic. Called once by the owning container.</summary>
    void Initialize();
}