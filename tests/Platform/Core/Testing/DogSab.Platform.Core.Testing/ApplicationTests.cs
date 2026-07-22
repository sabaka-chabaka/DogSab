using DogSab.Platform.Core.Abstractions.Application;
using DogSab.Platform.Core.Application.Application;
using DogSab.Platform.Core.Application.EntryPoints;
using FluentAssertions;
using Moq;

namespace DogSab.Platform.Core.Testing;

public class ApplicationTests
{
    [Fact]
    public void Start_ShouldInitializeApplicationAndReturnHandle()
    {
        // Act
        // Use ApplicationBuilder to avoid static Instance issues if possible, 
        // but DogSabApplication.Initialize is static and sets Instance.
        // We need to be careful with global state in tests.
        
        RunningApplication runningApp;
        try 
        {
            runningApp = ApplicationBuilder.Start();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already been initialized"))
        {
            // If already initialized by another test, we can't easily re-initialize.
            // In a real project we might need a way to reset the static instance for testing.
            return; 
        }

        // Assert
        runningApp.Should().NotBeNull();
        runningApp.Application.Should().BeSameAs(DogSabApplication.Instance);
        runningApp.ProjectSessionManager.Should().NotBeNull();
        runningApp.ShutdownCoordinator.Should().NotBeNull();

        runningApp.Application.MessageBus.Should().NotBeNull();
        runningApp.Application.LoggerFactory.Should().NotBeNull();
    }

    [Fact]
    public void NotifyExiting_ShouldPublishExitingEvent()
    {
        // Arrange
        // We assume Instance is already initialized by Start_ShouldInitializeApplicationAndReturnHandle
        // or we initialize it here.
        DogSabApplication app;
        try
        {
            app = DogSabApplication.Initialize();
        }
        catch (InvalidOperationException)
        {
            app = DogSabApplication.Instance;
        }

        var listenerMock = new Mock<IApplicationLifecycleListener>();
        using (var connection = app.MessageBus.Connect())
        {
            connection.Subscribe(ApplicationEventPublisher.Topic, listenerMock.Object);

            // Act
            app.NotifyExiting();

            // Assert
            listenerMock.Verify(x => x.OnLifecycleEvent(It.Is<ApplicationEventArgs>(e => e.Event == ApplicationLifecycleEvent.Exiting)), Times.Once);
        }
    }
}
