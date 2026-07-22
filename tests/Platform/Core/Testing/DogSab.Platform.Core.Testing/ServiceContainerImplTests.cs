using DogSab.Platform.Core.Abstractions.Exceptions;
using DogSab.Platform.Core.Abstractions.Services;
using DogSab.Platform.Core.Impl.Services;
using FluentAssertions;
using Moq;

namespace DogSab.Platform.Core.Testing;

public class ServiceContainerImplTests
{
    private readonly ServiceRegistry _registry;
    private readonly Mock<ServiceActivator> _activatorMock;
    private readonly ServiceContainerImpl _container;

    public ServiceContainerImplTests()
    {
        _registry = new ServiceRegistry();
        _activatorMock = new Mock<ServiceActivator>(new ServiceCircularDependencyDetector());
        _container = new ServiceContainerImpl(_registry, _activatorMock.Object);
    }

    public interface ITestService : IService { }
    public class TestService : ITestService { }

    [Fact]
    public void RegisterService_ShouldAddRegistrationToRegistry()
    {
        // Arrange
        var registration = new ServiceRegistration(typeof(ITestService), typeof(TestService), ServiceScope.Application, ServiceLifetime.Singleton);

        // Act
        _container.RegisterService(registration);

        // Assert
        _container.IsRegistered<ITestService>().Should().BeTrue();
    }

    [Fact]
    public void RegisterInstance_ShouldCacheAndRegisterInstance()
    {
        // Arrange
        var instance = new TestService();

        // Act
        _container.RegisterInstance<ITestService>(instance);

        // Assert
        _container.IsRegistered<ITestService>().Should().BeTrue();
        _container.GetService<ITestService>().Should().BeSameAs(instance);
    }

    [Fact]
    public void GetService_ShouldResolveViaActivator_WhenNotCached()
    {
        // Arrange
        var registration = new ServiceRegistration(typeof(ITestService), typeof(TestService), ServiceScope.Application, ServiceLifetime.Singleton);
        _container.RegisterService(registration);
        var instance = new TestService();
        _activatorMock.Setup(a => a.CreateInstance(registration, _container)).Returns(instance);

        // Act
        var result = _container.GetService<ITestService>();

        // Assert
        result.Should().BeSameAs(instance);
        _activatorMock.Verify(a => a.CreateInstance(registration, _container), Times.Once);
    }

    [Fact]
    public void GetService_ShouldReturnCachedInstance_OnSecondCall()
    {
        // Arrange
        var registration = new ServiceRegistration(typeof(ITestService), typeof(TestService), ServiceScope.Application, ServiceLifetime.Singleton);
        _container.RegisterService(registration);
        var instance = new TestService();
        _activatorMock.Setup(a => a.CreateInstance(registration, _container)).Returns(instance);

        // Act
        var first = _container.GetService<ITestService>();
        var second = _container.GetService<ITestService>();

        // Assert
        first.Should().BeSameAs(instance);
        second.Should().BeSameAs(instance);
        _activatorMock.Verify(a => a.CreateInstance(registration, _container), Times.Once);
    }

    [Fact]
    public void GetService_ShouldDelegateToParent_WhenNotRegisteredLocally()
    {
        // Arrange
        var parentRegistry = new ServiceRegistry();
        var parentContainer = new ServiceContainerImpl(parentRegistry, _activatorMock.Object);
        var childContainer = new ServiceContainerImpl(ServiceScope.Project, new ServiceRegistry(), _activatorMock.Object, parentContainer);

        var registration = new ServiceRegistration(typeof(ITestService), typeof(TestService), ServiceScope.Application, ServiceLifetime.Singleton);
        parentContainer.RegisterService(registration);
        var instance = new TestService();
        _activatorMock.Setup(a => a.CreateInstance(registration, parentContainer)).Returns(instance);

        // Act
        var result = childContainer.GetService<ITestService>();

        // Assert
        result.Should().BeSameAs(instance);
    }

    [Fact]
    public void GetService_ShouldThrow_WhenNotFoundAnywhere()
    {
        // Act
        var act = () => _container.GetService<ITestService>();

        // Assert
        act.Should().Throw<ServiceResolutionException>();
    }

    [Fact]
    public void TryGetService_ShouldReturnFalse_WhenNotFound()
    {
        // Act
        var result = _container.TryGetService<ITestService>(out var service);

        // Assert
        result.Should().BeFalse();
        service.Should().BeNull();
    }

    [Fact]
    public void TryGetService_ShouldReturnTrue_WhenFound()
    {
        // Arrange
        var instance = new TestService();
        _container.RegisterInstance<ITestService>(instance);

        // Act
        var result = _container.TryGetService<ITestService>(out var service);

        // Assert
        result.Should().BeTrue();
        service.Should().BeSameAs(instance);
    }

    [Fact]
    public void ClearCache_ShouldForceReactivation()
    {
        // Arrange
        var registration = new ServiceRegistration(typeof(ITestService), typeof(TestService), ServiceScope.Application, ServiceLifetime.Singleton);
        _container.RegisterService(registration);
        var instance1 = new TestService();
        var instance2 = new TestService();
        _activatorMock.SetupSequence(a => a.CreateInstance(registration, _container))
            .Returns(instance1)
            .Returns(instance2);

        // Act
        var first = _container.GetService<ITestService>();
        _container.ClearCache();
        var second = _container.GetService<ITestService>();

        // Assert
        first.Should().BeSameAs(instance1);
        second.Should().BeSameAs(instance2);
        _activatorMock.Verify(a => a.CreateInstance(registration, _container), Times.Exactly(2));
    }
}
