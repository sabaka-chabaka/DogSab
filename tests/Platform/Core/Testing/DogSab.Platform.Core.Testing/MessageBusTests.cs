using DogSab.Platform.Core.Abstractions.Logging;
using DogSab.Platform.Core.Abstractions.Messaging;
using DogSab.Platform.Core.Abstractions.Threading;
using DogSab.Platform.Core.Messaging.Impl.Bus;
using FluentAssertions;
using Moq;

namespace DogSab.Platform.Core.Testing;

public class MessageBusTests
{
    private readonly MessageBusImpl _bus;
    private readonly Mock<IUiThreadDispatcher> _dispatcherMock;
    private readonly Mock<ILoggerFactory> _loggerFactoryMock;

    public MessageBusTests()
    {
        _dispatcherMock = new Mock<IUiThreadDispatcher>();
        _loggerFactoryMock = new Mock<ILoggerFactory>();
        _loggerFactoryMock.Setup(x => x.GetLogger(It.IsAny<Type>())).Returns(new Mock<ILogger>().Object);

        _bus = new MessageBusImpl(_dispatcherMock.Object, _loggerFactoryMock.Object);
    }

    public interface ITestListener
    {
        void OnEvent(string message);
    }

    private class TestTopic : ITopic<ITestListener>
    {
        public string Id => "TestTopic";
        public string DisplayName => Id;
        public DeliveryMode DeliveryMode => DeliveryMode.Synchronous;
    }

    [Fact]
    public void Publisher_ShouldBroadcastToSubscribers()
    {
        // Arrange
        var topic = new TestTopic();
        var listenerMock = new Mock<ITestListener>();
        using var connection = _bus.Connect();
        connection.Subscribe(topic, listenerMock.Object);

        // Act
        var publisher = _bus.Publisher(topic);
        publisher.OnEvent("Hello");

        // Assert
        listenerMock.Verify(x => x.OnEvent("Hello"), Times.Once);
    }

    [Fact]
    public void Unsubscribe_ShouldStopBroadcasting()
    {
        // Arrange
        var topic = new TestTopic();
        var listenerMock = new Mock<ITestListener>();
        
        using (var connection = _bus.Connect())
        {
            connection.Subscribe(topic, listenerMock.Object);
        } // connection disposed -> unsubscribed

        // Act
        var publisher = _bus.Publisher(topic);
        publisher.OnEvent("Hello");

        // Assert
        listenerMock.Verify(x => x.OnEvent(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void Publisher_ShouldHandleMultipleSubscribers()
    {
        // Arrange
        var topic = new TestTopic();
        var listenerMock1 = new Mock<ITestListener>();
        var listenerMock2 = new Mock<ITestListener>();
        using var connection = _bus.Connect();
        connection.Subscribe(topic, listenerMock1.Object);
        connection.Subscribe(topic, listenerMock2.Object);

        // Act
        _bus.Publisher(topic).OnEvent("Multi");

        // Assert
        listenerMock1.Verify(x => x.OnEvent("Multi"), Times.Once);
        listenerMock2.Verify(x => x.OnEvent("Multi"), Times.Once);
    }
}
