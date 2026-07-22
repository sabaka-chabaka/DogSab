using DogSab.Platform.Core.Abstractions.Progress;
using DogSab.Platform.Core.Progress.Impl.Manager;
using FluentAssertions;

namespace DogSab.Platform.Core.Testing;

public class ProgressManagerTests
{
    private readonly ProgressManagerImpl _manager;

    public ProgressManagerTests()
    {
        _manager = new ProgressManagerImpl();
    }

    [Fact]
    public void RunWithProgress_ShouldSetAndClearCurrentProgress()
    {
        // Act & Assert
        _manager.GetCurrentProgress().Should().BeNull();

        _manager.RunWithProgress("Test", indicator =>
        {
            _manager.GetCurrentProgress().Should().BeSameAs(indicator);
            indicator.Text.Should().Be("Test");
        });

        _manager.GetCurrentProgress().Should().BeNull();
    }

    [Fact]
    public async Task RunWithProgressAsync_ShouldSetAndClearCurrentProgress()
    {
        // Act & Assert
        _manager.GetCurrentProgress().Should().BeNull();

        await _manager.RunWithProgressAsync("Test Async", async indicator =>
        {
            await Task.Yield();
            _manager.GetCurrentProgress().Should().BeSameAs(indicator);
        });

        _manager.GetCurrentProgress().Should().BeNull();
    }

    [Fact]
    public void RunWithProgress_ShouldClearProgress_WhenActionThrows()
    {
        // Act
        var act = () => _manager.RunWithProgress("Error", _ => throw new Exception("fail"));

        // Assert
        act.Should().Throw<Exception>();
        _manager.GetCurrentProgress().Should().BeNull();
    }

    [Fact]
    public void RunWithProgress_ShouldSupportNestedCalls()
    {
        _manager.RunWithProgress("Outer", outer =>
        {
            _manager.GetCurrentProgress().Should().BeSameAs(outer);

            _manager.RunWithProgress("Inner", inner =>
            {
                _manager.GetCurrentProgress().Should().BeSameAs(inner);
                inner.Should().NotBeSameAs(outer);
            });

            _manager.GetCurrentProgress().Should().BeSameAs(outer);
        });
    }
}
