using DogSab.Platform.Core.Abstractions.Lifecycle;
using DogSab.Platform.Core.Abstractions.Logging;
using DogSab.Platform.Core.Impl.Lifecycle;
using FluentAssertions;
using Moq;

namespace DogSab.Platform.Core.Testing;

public class StartupActivityRunnerTests
{
    private readonly Mock<ILoggerFactory> _loggerFactoryMock;
    private readonly Mock<ILogger> _loggerMock;
    private readonly LifecycleOrderResolver _orderResolver;
    private readonly StartupActivityRunner _runner;

    public StartupActivityRunnerTests()
    {
        _loggerFactoryMock = new Mock<ILoggerFactory>();
        _loggerMock = new Mock<ILogger>();
        _loggerFactoryMock.Setup(f => f.GetLogger(It.IsAny<Type>())).Returns(_loggerMock.Object);
        _orderResolver = new LifecycleOrderResolver();
        _runner = new StartupActivityRunner(_loggerFactoryMock.Object, _orderResolver);
    }

    public class TestActivity : IStartupActivity
    {
        private readonly Func<Task> _action;
        public int Order { get; }
        public bool WasRun { get; private set; }

        public TestActivity(int order, Func<Task>? action = null)
        {
            Order = order;
            _action = action ?? (() => Task.CompletedTask);
        }

        public async Task RunActivityAsync(CancellationToken cancellationToken)
        {
            await _action();
            WasRun = true;
        }
    }

    [Fact]
    public async Task RunAllAsync_ShouldRunActivitiesInOrder()
    {
        // Arrange
        var executionOrder = new List<int>();
        var activities = new[]
        {
            new TestActivity(10, () => { executionOrder.Add(10); return Task.CompletedTask; }),
            new TestActivity(1, () => { executionOrder.Add(1); return Task.CompletedTask; }),
            new TestActivity(5, () => { executionOrder.Add(5); return Task.CompletedTask; })
        };

        // Act
        await _runner.RunAllAsync(activities, CancellationToken.None);

        // Assert
        executionOrder.Should().Equal(1, 5, 10);
        _runner.CompletedActivities.Should().HaveCount(3);
    }

    [Fact]
    public async Task RunAllAsync_ShouldStopAtFirstFailure()
    {
        // Arrange
        var activities = new[]
        {
            new TestActivity(1),
            new TestActivity(2, () => throw new InvalidOperationException("Failure")),
            new TestActivity(3)
        };

        // Act
        var act = () => _runner.RunAllAsync(activities, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Failure");
        _runner.CompletedActivities.Should().HaveCount(1);
        activities[0].WasRun.Should().BeTrue();
        activities[1].WasRun.Should().BeFalse();
        activities[2].WasRun.Should().BeFalse();
    }

    [Fact]
    public async Task RunAllAsync_ShouldRespectCancellationToken()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var activities = new[]
        {
            new TestActivity(1, async () => { cts.Cancel(); await Task.Yield(); }),
            new TestActivity(2)
        };

        // Act
        var act = () => _runner.RunAllAsync(activities, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
        _runner.CompletedActivities.Should().HaveCount(1);
        activities[0].WasRun.Should().BeTrue();
        activities[1].WasRun.Should().BeFalse();
    }
}
