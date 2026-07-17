namespace DogSab.Platform.Core.Abstractions.Settings;

/// <summary>
/// A component whose state is automatically serialized/deserialized
/// by the platform at startup/shutdown (analogous to PersistentStateComponent in IJ).
/// </summary>
/// <typeparam name="TState">The plain-data type representing the component's persisted state.</typeparam>
public interface IPersistentStateComponent<TState> where TState : class, new()
{
    /// <summary>
    /// Returns the current state to be persisted. Called by the platform before saving.
    /// </summary>
    /// <returns>The current state snapshot.</returns>
    TState GetState();

    /// <summary>
    /// Restores the component from a previously persisted state. Called by the platform after loading.
    /// </summary>
    /// <param name="state">The state to restore from.</param>
    void LoadState(TState state);
}