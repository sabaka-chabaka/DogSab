using DogSab.Platform.Core.Impl.Services;
using FluentAssertions;

namespace DogSab.Platform.Core.Testing;

public class ServiceCircularDependencyDetectorTests
{
    private readonly ServiceCircularDependencyDetector _detector;

    public ServiceCircularDependencyDetectorTests()
    {
        _detector = new ServiceCircularDependencyDetector();
    }

    [Fact]
    public void Enter_ShouldNotThrow_WhenTypeNotAlreadyInStack()
    {
        // Act
        var act = () => _detector.Enter(typeof(string));

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Enter_ShouldThrow_WhenTypeAlreadyInStack()
    {
        // Arrange
        _detector.Enter(typeof(int));
        _detector.Enter(typeof(string));

        // Act
        var act = () => _detector.Enter(typeof(int));

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Circular service dependency detected*Int32 -> String -> Int32*");
    }

    [Fact]
    public void Exit_ShouldRemoveFromStack()
    {
        // Arrange
        _detector.Enter(typeof(int));
        _detector.Exit(typeof(int));

        // Act
        var act = () => _detector.Enter(typeof(int));

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Exit_ShouldDoNothing_WhenStackEmpty()
    {
        // Act
        var act = () => _detector.Exit(typeof(int));

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Exit_ShouldDoNothing_WhenTopIsNotTarget()
    {
        // Arrange
        _detector.Enter(typeof(int));
        _detector.Enter(typeof(string));
        
        // Act
        _detector.Exit(typeof(int)); // int is not at top, string is

        // Assert
        var act = () => _detector.Enter(typeof(string));
        act.Should().Throw<InvalidOperationException>(); // string should still be there
    }
}
