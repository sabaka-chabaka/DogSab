using DogSab.Platform.Core.Abstractions.Services;
using DogSab.Platform.Core.Impl.Services;
using FluentAssertions;
using Moq;

namespace DogSab.Platform.Core.Testing;

public class ServiceActivatorTests
{
    private readonly Mock<ServiceCircularDependencyDetector> _detectorMock;
    private readonly ServiceActivator _activator;
    private readonly Mock<IServiceContainer> _containerMock;

    public ServiceActivatorTests()
    {
        _detectorMock = new Mock<ServiceCircularDependencyDetector>();
        _activator = new ServiceActivator(_detectorMock.Object);
        _containerMock = new Mock<IServiceContainer>();
    }

    public class NoPublicConstructor
    {
        private NoPublicConstructor() { }
    }

    public class SingleConstructor
    {
        public bool Called { get; }
        public SingleConstructor() => Called = true;
    }

    public class MultipleConstructors
    {
        public int Chosen { get; }
        public MultipleConstructors() => Chosen = 1;
        public MultipleConstructors(string a) => Chosen = 2;
        public MultipleConstructors(string a, int b) => Chosen = 3;
    }

    public class DependencyConstructor
    {
        public string Dependency { get; }
        public DependencyConstructor(string dependency) => Dependency = dependency;
    }

    [Fact]
    public void CreateInstance_ShouldThrow_WhenNoPublicConstructors()
    {
        // Arrange
        var registration = new ServiceRegistration(typeof(object), typeof(NoPublicConstructor), ServiceScope.Application, ServiceLifetime.Singleton);

        // Act
        var act = () => _activator.CreateInstance(registration, _containerMock.Object);

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("*has no public constructors*");
    }

    [Fact]
    public void CreateInstance_ShouldCallDefaultConstructor()
    {
        // Arrange
        var registration = new ServiceRegistration(typeof(SingleConstructor), typeof(SingleConstructor), ServiceScope.Application, ServiceLifetime.Singleton);

        // Act
        var result = _activator.CreateInstance(registration, _containerMock.Object);

        // Assert
        result.Should().BeOfType<SingleConstructor>();
        ((SingleConstructor)result).Called.Should().BeTrue();
    }

    [Fact]
    public void CreateInstance_ShouldSelectConstructorWithMostParameters()
    {
        // Arrange
        var registration = new ServiceRegistration(typeof(MultipleConstructors), typeof(MultipleConstructors), ServiceScope.Application, ServiceLifetime.Singleton);
        _containerMock.Setup(c => c.GetService(typeof(string))).Returns("test");
        _containerMock.Setup(c => c.GetService(typeof(int))).Returns(42);

        // Act
        var result = _activator.CreateInstance(registration, _containerMock.Object);

        // Assert
        result.Should().BeOfType<MultipleConstructors>();
        ((MultipleConstructors)result).Chosen.Should().Be(3);
    }

    [Fact]
    public void CreateInstance_ShouldInjectDependenciesFromContainer()
    {
        // Arrange
        var registration = new ServiceRegistration(typeof(DependencyConstructor), typeof(DependencyConstructor), ServiceScope.Application, ServiceLifetime.Singleton);
        _containerMock.Setup(c => c.GetService(typeof(string))).Returns("resolved-dependency");

        // Act
        var result = _activator.CreateInstance(registration, _containerMock.Object);

        // Assert
        result.Should().BeOfType<DependencyConstructor>();
        ((DependencyConstructor)result).Dependency.Should().Be("resolved-dependency");
    }

    [Fact]
    public void CreateInstance_ShouldEnterAndExitDetector()
    {
        // Arrange
        var registration = new ServiceRegistration(typeof(SingleConstructor), typeof(SingleConstructor), ServiceScope.Application, ServiceLifetime.Singleton);

        // Act
        _activator.CreateInstance(registration, _containerMock.Object);

        // Assert
        _detectorMock.Verify(d => d.Enter(typeof(SingleConstructor)), Times.Once);
        _detectorMock.Verify(d => d.Exit(typeof(SingleConstructor)), Times.Once);
    }
}
