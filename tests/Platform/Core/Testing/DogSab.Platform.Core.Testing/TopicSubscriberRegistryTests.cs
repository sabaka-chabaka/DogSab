using DogSab.Platform.Core.Abstractions.Messaging;
using DogSab.Platform.Core.Messaging.Impl.Bus;
using FluentAssertions;

namespace DogSab.Platform.Core.Testing;

public class TopicSubscriberRegistryTests
{
    private readonly TopicSubscriberRegistry _registry;

    public TopicSubscriberRegistryTests()
    {
        _registry = new TopicSubscriberRegistry();
    }

    public interface ITestListener
    {
        void OnEvent();
    }

    private class TestTopic : ITopic<ITestListener>
    {
        public string Id { get; init; } = "TestTopic";
        public string DisplayName => Id;
        public DeliveryMode DeliveryMode => DeliveryMode.Synchronous;
    }

    [Fact]
    public void Subscribe_ShouldAddListener()
    {
        // Arrange
        var topic = new TestTopic();
        var listener = new object();

        // Act
        _registry.Subscribe(topic, listener);

        // Assert
        var subscribers = _registry.GetLiveSubscribers(topic);
        subscribers.Should().ContainSingle().Which.Should().BeSameAs(listener);
    }

    [Fact]
    public void Unsubscribe_ShouldRemoveListener()
    {
        // Arrange
        var topic = new TestTopic();
        var listener = new object();
        _registry.Subscribe(topic, listener);

        // Act
        _registry.Unsubscribe(topic, listener);

        // Assert
        _registry.GetLiveSubscribers(topic).Should().BeEmpty();
    }

    [Fact]
    public void GetLiveSubscribers_ShouldCleanUpCollectedListeners()
    {
        // Arrange
        var topic = new TestTopic();
        SubscribeWeak(topic);

        // Act
        GC.Collect();
        GC.WaitForPendingFinalizers();
        var subscribers = _registry.GetLiveSubscribers(topic);

        // Assert
        subscribers.Should().BeEmpty();
    }

    // Helper method to ensure listener is eligible for collection
    private void SubscribeWeak(ITopic topic)
    {
        var listener = new object();
        _registry.Subscribe(topic, listener);
    }

    [Fact]
    public void UnsubscribeFromAllTopics_ShouldRemoveListenerEverywhere()
    {
        // Arrange
        var topic1 = new TestTopic { Id = "T1" };
        var topic2 = new TestTopic { Id = "T2" };
        var listener = new object();
        
        _registry.Subscribe(topic1, listener);
        _registry.Subscribe(topic2, listener);

        // Act
        _registry.UnsubscribeFromAllTopics(listener);

        // Assert
        _registry.GetLiveSubscribers(topic1).Should().BeEmpty();
        _registry.GetLiveSubscribers(topic2).Should().BeEmpty();
    }

    private class TestTopicWithId : ITopic
    {
        public string Id { get; init; } = "";
        public string DisplayName => Id;
        public DeliveryMode DeliveryMode => DeliveryMode.Synchronous;
    }
}

// Need to allow setting Id for testing multiple topics
internal class TestTopic : ITopic<TopicSubscriberRegistryTests.ITestListener>
{
    public string Id { get; init; } = "TestTopic";
    public string DisplayName => Id;
    public DeliveryMode DeliveryMode => DeliveryMode.Synchronous;
}
