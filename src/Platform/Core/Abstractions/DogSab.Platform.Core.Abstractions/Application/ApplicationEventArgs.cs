namespace DogSab.Platform.Core.Abstractions.Application;

/// <summary>Event arguments carrying an application lifecycle event.</summary>
public class ApplicationEventArgs : EventArgs
{
    /// <summary>The lifecycle event that occurred.</summary>
    public ApplicationLifecycleEvent Event { get; }
    
    /// <summary>
    /// Creates new arguments for the given lifecycle event.
    /// </summary>
    /// <param name="event">The lifecycle event that occurred.</param>
    public ApplicationEventArgs(ApplicationLifecycleEvent @event) => Event = @event;
}