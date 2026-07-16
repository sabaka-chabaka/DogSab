namespace DogSab.Platform.Core.Abstractions.Components;

public enum ComponentLifecycleState
{
    NotInitialized,
    Initializing,
    Initialized,
    Disposing,
    Disposed
}