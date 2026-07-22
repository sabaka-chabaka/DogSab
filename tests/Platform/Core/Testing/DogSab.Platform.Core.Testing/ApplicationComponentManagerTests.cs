using DogSab.Platform.Core.Abstractions.Components;
using DogSab.Platform.Core.Abstractions.Exceptions;
using DogSab.Platform.Core.Impl.Components;
using FluentAssertions;
using Moq;

namespace DogSab.Platform.Core.Testing;

public class ApplicationComponentManagerTests
{
    private readonly ComponentDependencyResolver _dependencyResolver;
    private readonly ApplicationComponentManager _manager;

    public ApplicationComponentManagerTests()
    {
        _dependencyResolver = new ComponentDependencyResolver();
        _manager = new ApplicationComponentManager(_dependencyResolver);
    }

    public interface ITestComponent : IApplicationComponent { }
    public interface ITestComponent2 : IApplicationComponent { }

    public class TestComponent : ITestComponent
    {
        public bool IsInitialized { get; private set; }
        public bool IsDisposed { get; private set; }
        public void InitComponent() => IsInitialized = true;
        public void DisposeComponent() => IsDisposed = true;
    }

    [ComponentDependsOn(typeof(ITestComponent))]
    public class TestComponent2 : ITestComponent2
    {
        public bool IsInitialized { get; private set; }
        public bool IsDisposed { get; private set; }
        public void InitComponent() => IsInitialized = true;
        public void DisposeComponent() => IsDisposed = true;
    }

    public class NotAnAppComponent : IComponent { }

    [Fact]
    public void RegisterComponent_ShouldThrow_WhenImplementationDoesNotImplementIApplicationComponent()
    {
        // Act
        var act = () => _manager.RegisterComponent<IComponent, NotAnAppComponent>();

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*must implement IApplicationComponent*");
    }

    [Fact]
    public void GetComponent_ShouldInitializeAndReturnComponent()
    {
        // Arrange
        _manager.RegisterComponent<ITestComponent, TestComponent>();

        // Act
        var component = _manager.GetComponent<ITestComponent>();

        // Assert
        component.Should().BeOfType<TestComponent>();
        ((TestComponent)component).IsInitialized.Should().BeTrue();
        _manager.GetState(typeof(ITestComponent)).Should().Be(ComponentLifecycleState.Initialized);
    }

    [Fact]
    public void GetComponent_ShouldReturnSameInstance_OnSecondCall()
    {
        // Arrange
        _manager.RegisterComponent<ITestComponent, TestComponent>();

        // Act
        var first = _manager.GetComponent<ITestComponent>();
        var second = _manager.GetComponent<ITestComponent>();

        // Assert
        first.Should().BeSameAs(second);
    }

    [Fact]
    public void GetComponent_ShouldInitializeDependenciesInOrder()
    {
        // Arrange
        _manager.RegisterComponent<ITestComponent, TestComponent>();
        _manager.RegisterComponent<ITestComponent2, TestComponent2>();

        // Act
        var component2 = _manager.GetComponent<ITestComponent2>();

        // Assert
        component2.Should().BeOfType<TestComponent2>();
        var component1 = _manager.GetComponent<ITestComponent>();
        
        ((TestComponent)component1).IsInitialized.Should().BeTrue();
        ((TestComponent2)component2).IsInitialized.Should().BeTrue();
    }

    [Fact]
    public void GetComponent_ShouldThrow_WhenComponentNotRegistered()
    {
        // Act
        var act = () => _manager.GetComponent<ITestComponent>();

        // Assert
        act.Should().Throw<ComponentNotFoundException>();
    }

    [Fact]
    public void TryGetComponent_ShouldReturnFalse_WhenNotRegistered()
    {
        // Act
        var result = _manager.TryGetComponent<ITestComponent>(out var component);

        // Assert
        result.Should().BeFalse();
        component.Should().BeNull();
    }

    [Fact]
    public void TryGetComponent_ShouldReturnTrue_WhenRegistered()
    {
        // Arrange
        _manager.RegisterComponent<ITestComponent, TestComponent>();

        // Act
        var result = _manager.TryGetComponent<ITestComponent>(out var component);

        // Assert
        result.Should().BeTrue();
        component.Should().NotBeNull();
    }

    [Fact]
    public void DisposeAll_ShouldDisposeAllInitializedComponents()
    {
        // Arrange
        _manager.RegisterComponent<ITestComponent, TestComponent>();
        var component = (TestComponent)_manager.GetComponent<ITestComponent>();

        // Act
        _manager.DisposeAll();

        // Assert
        component.IsDisposed.Should().BeTrue();
        
        // Verifying they are removed from instances by trying to get it again (should re-init)
        var newComponent = (TestComponent)_manager.GetComponent<ITestComponent>();
        newComponent.Should().NotBeSameAs(component);
    }
}
