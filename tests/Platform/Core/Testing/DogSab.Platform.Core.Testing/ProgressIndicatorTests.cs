using DogSab.Platform.Core.Progress.Impl.Indicators;
using FluentAssertions;

namespace DogSab.Platform.Core.Testing;

public class ProgressIndicatorTests
{
    private readonly ProgressIndicatorImpl _indicator;

    public ProgressIndicatorTests()
    {
        _indicator = new ProgressIndicatorImpl();
    }

    [Fact]
    public void Fraction_ShouldBeClamped()
    {
        // Act
        _indicator.Fraction = 1.5;
        var high = _indicator.Fraction;
        
        _indicator.Fraction = -0.5;
        var low = _indicator.Fraction;

        // Assert
        high.Should().Be(1.0);
        low.Should().Be(0.0);
    }

    [Fact]
    public void Cancel_ShouldSetIsCanceledAndRaiseEvent()
    {
        // Arrange
        var eventRaised = false;
        _indicator.Canceled += (s, e) => eventRaised = true;

        // Act
        _indicator.Cancel();

        // Assert
        _indicator.IsCanceled.Should().BeTrue();
        eventRaised.Should().BeTrue();
    }

    [Fact]
    public void Cancel_ShouldNotRaiseEventTwice()
    {
        // Arrange
        var raiseCount = 0;
        _indicator.Canceled += (s, e) => raiseCount++;

        // Act
        _indicator.Cancel();
        _indicator.Cancel();

        // Assert
        raiseCount.Should().Be(1);
    }

    [Fact]
    public void CheckCanceled_ShouldThrow_WhenCanceled()
    {
        // Arrange
        _indicator.Cancel();

        // Act
        var act = () => _indicator.CheckCanceled();

        // Assert
        act.Should().Throw<OperationCanceledException>();
    }

    [Fact]
    public void TextProperties_ShouldHandleNull()
    {
        // Act
        _indicator.Text = null!;
        _indicator.SecondaryText = null!;

        // Assert
        _indicator.Text.Should().BeEmpty();
        _indicator.SecondaryText.Should().BeEmpty();
    }
}
