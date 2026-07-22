using DogSab.Platform.Core.Abstractions.Application;
using DogSab.Platform.Core.Abstractions.Logging;
using DogSab.Platform.Core.Abstractions.Messaging;
using DogSab.Platform.Core.Application.Application;
using DogSab.Platform.Core.Application.ProjectLifecycle;
using DogSab.Platform.Core.Application.Shutdown;
using DogSab.Platform.Core.Impl.Components;
using DogSab.Platform.Core.Impl.Disposables;
using Moq;

namespace DogSab.Platform.Core.Testing;

public class ApplicationShutdownCoordinatorTests
{
    private readonly Mock<ProjectSessionManager> _projectSessionManagerMock;
    private readonly Mock<ApplicationComponentManager> _applicationComponentManagerMock;
    private readonly Mock<DisposableRegistryImpl> _disposableRegistryMock;
    private readonly Mock<ApplicationEventPublisher> _eventPublisherMock;
    private readonly Mock<ILoggerFactory> _loggerFactoryMock;
    private readonly Mock<ILogger> _loggerMock;
    private readonly ApplicationShutdownCoordinator _coordinator;

    public ApplicationShutdownCoordinatorTests()
    {
        _loggerFactoryMock = new Mock<ILoggerFactory>();
        _loggerMock = new Mock<ILogger>();
        _loggerFactoryMock.Setup(f => f.GetLogger(It.IsAny<Type>())).Returns(_loggerMock.Object);

        // We need to provide dependencies for the constructors because they are concrete types
        var dependencyResolver = new ComponentDependencyResolver();
        _projectSessionManagerMock = new Mock<ProjectSessionManager>(null!, _loggerFactoryMock.Object);
        _applicationComponentManagerMock = new Mock<ApplicationComponentManager>(dependencyResolver);
        _disposableRegistryMock = new Mock<DisposableRegistryImpl>(_loggerFactoryMock.Object);
        _eventPublisherMock = new Mock<ApplicationEventPublisher>(Mock.Of<IMessageBus>());

        _coordinator = new ApplicationShutdownCoordinator(
            _projectSessionManagerMock.Object,
            _applicationComponentManagerMock.Object,
            _disposableRegistryMock.Object,
            _eventPublisherMock.Object,
            _loggerFactoryMock.Object);
    }

    [Fact]
    public void Shutdown_ShouldRunAllStepsInOrder()
    {
        // Arrange
        var sequence = new MockSequence();
        _eventPublisherMock.InSequence(sequence).Setup(p => p.Publish(ApplicationLifecycleEvent.Exiting));
        _projectSessionManagerMock.InSequence(sequence).Setup(m => m.CloseAllProjects());
        _applicationComponentManagerMock.InSequence(sequence).Setup(m => m.DisposeAll());

        // Act
        _coordinator.Shutdown(ShutdownReason.UserRequested);

        // Assert
        _eventPublisherMock.Verify(p => p.Publish(ApplicationLifecycleEvent.Exiting), Times.Once);
        _projectSessionManagerMock.Verify(m => m.CloseAllProjects(), Times.Once);
        _applicationComponentManagerMock.Verify(m => m.DisposeAll(), Times.Once);
    }

    [Fact]
    public void Shutdown_ShouldContinueEvenIfStepFails()
    {
        // Arrange
        _eventPublisherMock.Setup(p => p.Publish(It.IsAny<ApplicationLifecycleEvent>()))
            .Throws(new Exception("Event failed"));
        _projectSessionManagerMock.Setup(m => m.CloseAllProjects())
            .Throws(new Exception("Projects failed"));

        // Act
        _coordinator.Shutdown(ShutdownReason.ExternalSignal);

        // Assert
        _applicationComponentManagerMock.Verify(m => m.DisposeAll(), Times.Once);
        _loggerMock.Verify(l => l.Error(It.Is<string>(s => s.Contains("failed")), It.IsAny<Exception>(), It.IsAny<string>()), Times.AtLeast(2));
    }
}
