using DogSab.Platform.Core.Abstractions.Application;
using DogSab.Platform.Core.Abstractions.Messaging;
using DogSab.Platform.Core.Abstractions.Services;

namespace DogSab.Platform.Core.Application.Application;

/// <summary>
/// Listener interface for application lifecycle events, published on the
/// <see cref="ApplicationEventPublisher.Topic"/> topic. Platform subsystems and
/// plugins subscribe to this to react to startup, shutdown, and project
/// open/close transitions without polling application state directly.
/// </summary>
public interface IApplicationLifecycleListener
{
    /// <summary>
    /// Called whenever the application transitions to a new lifecycle stage.
    /// </summary>
    /// <param name="args">Details of the lifecycle event that occurred.</param>
    void OnLifecycleEvent(ApplicationEventArgs args);
}

/// <summary>
/// Publishes <see cref="ApplicationLifecycleEvent"/> transitions to the message
/// bus, so any platform subsystem or plugin can observe application startup,
/// shutdown, and project open/close transitions via
/// <see cref="IApplicationLifecycleListener"/> without depending directly on
/// <see cref="DogSabApplication"/> or <see cref="ProjectLifecycle.ProjectSessionManager"/>.
/// </summary>
public class ApplicationEventPublisher : IService
{
    /// <summary>
    /// The topic application lifecycle events are published on. Delivered
    /// synchronously, on whatever thread the transition occurred on — most
    /// lifecycle transitions already happen on a controlled thread (startup,
    /// shutdown) so UI-thread marshalling is left to individual subscribers
    /// that need it, rather than forced on every listener.
    /// </summary>
    public static readonly ITopic<IApplicationLifecycleListener> Topic =
        Messaging.Impl.Topics.TopicImpl<IApplicationLifecycleListener>.Create("ApplicationLifecycle");

    /// <summary>The message bus this publisher sends events through.</summary>
    private readonly IMessageBus _messageBus;

    /// <summary>
    /// Creates a new application event publisher.
    /// </summary>
    /// <param name="messageBus">The message bus to publish lifecycle events through.</param>
    public ApplicationEventPublisher(IMessageBus messageBus)
    {
        _messageBus = messageBus;
    }

    /// <summary>
    /// Publishes a lifecycle event to every current subscriber of <see cref="Topic"/>.
    /// </summary>
    /// <param name="lifecycleEvent">The lifecycle stage being transitioned to.</param>
    public virtual void Publish(ApplicationLifecycleEvent lifecycleEvent)
    {
        var args = new ApplicationEventArgs(lifecycleEvent);
        _messageBus.Publisher(Topic).OnLifecycleEvent(args);
    }
}