namespace DogSab.Platform.RunConfigurations.Abstractions.Events;

/// <summary>
/// Listener interface for run process lifecycle notifications, published
/// on <see cref="RunExtensionPoints.RUN_STATE_CHANGED"/>. Distinct from
/// subscribing directly to a single <see cref="IRunProcessHandle"/>'s own
/// <c>StateChanged</c> event — that lets code watching one specific run
/// react to it, while this topic-based listener lets platform-wide
/// subscribers (e.g. a future "Run" tool window listing every active
/// process) observe every run across the whole session without needing a
/// reference to each individual handle.
/// </summary>
public interface IRunListener
{
    /// <summary>
    /// Called whenever any launched process's state changes.
    /// </summary>
    /// <param name="handle">
    /// The process handle whose state changed.
    /// </param>
    void RunStateChanged(IRunProcessHandle handle);
}