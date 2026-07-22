using DogSab.Platform.Core.Abstractions.Components;
using DogSab.Platform.Core.Abstractions.Exceptions;
using DogSab.Platform.Core.Impl.Components;
using FluentAssertions;

namespace DogSab.Platform.Core.Testing;

public class ProjectComponentManagerTests
{
    private readonly ComponentDependencyResolver _dependencyResolver;
    private readonly ProjectComponentManager _manager;

    public ProjectComponentManagerTests()
    {
        _dependencyResolver = new ComponentDependencyResolver();
        _manager = new ProjectComponentManager(_dependencyResolver);
    }

    public interface ITestProjectComponent : IProjectComponent { }

    public class TestProjectComponent : ITestProjectComponent
    {
        public bool IsInitialized { get; private set; }
        public bool IsOpened { get; private set; }
        public bool IsClosed { get; private set; }
        public bool IsDisposed { get; private set; }

        public void InitComponent() => IsInitialized = true;
        public void ProjectOpened() => IsOpened = true;
        public void ProjectClosed() => IsClosed = true;
        public void DisposeComponent() => IsDisposed = true;
    }

    [Fact]
    public void RegisterComponent_ShouldThrow_WhenImplementationDoesNotImplementIProjectComponent()
    {
        // Act
        var act = () => _manager.RegisterComponent<IComponent, ApplicationComponentManagerTests.NotAnAppComponent>();

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*must implement IProjectComponent*");
    }

    [Fact]
    public void GetComponent_ShouldInitializeAndReturnComponent()
    {
        // Arrange
        _manager.RegisterComponent<ITestProjectComponent, TestProjectComponent>();

        // Act
        var component = _manager.GetComponent<ITestProjectComponent>();

        // Assert
        component.Should().BeOfType<TestProjectComponent>();
        ((TestProjectComponent)component).IsInitialized.Should().BeTrue();
        ((TestProjectComponent)component).IsOpened.Should().BeFalse(); // Not opened yet
        _manager.GetState(typeof(ITestProjectComponent)).Should().Be(ComponentLifecycleState.Initialized);
    }

    [Fact]
    public void NotifyProjectOpened_ShouldCallProjectOpenedOnAllInitializedComponents()
    {
        // Arrange
        _manager.RegisterComponent<ITestProjectComponent, TestProjectComponent>();
        var component = (TestProjectComponent)_manager.GetComponent<ITestProjectComponent>();

        // Act
        _manager.NotifyProjectOpened();

        // Assert
        component.IsOpened.Should().BeTrue();
    }

    [Fact]
    public void DisposeAll_ShouldCallProjectClosedAndDisposeOnAllInitializedComponents()
    {
        // Arrange
        _manager.RegisterComponent<ITestProjectComponent, TestProjectComponent>();
        var component = (TestProjectComponent)_manager.GetComponent<ITestProjectComponent>();
        _manager.NotifyProjectOpened();

        // Act
        _manager.DisposeAll();

        // Assert
        component.IsClosed.Should().BeTrue();
        component.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public void TryGetComponent_ShouldReturnFalse_WhenNotRegistered()
    {
        // Act
        var result = _manager.TryGetComponent<ITestProjectComponent>(out var component);

        // Assert
        result.Should().BeFalse();
        component.Should().BeNull();
    }

    [Fact]
    public void TryGetComponent_ShouldReturnTrue_WhenRegistered()
    {
        // Arrange
        _manager.RegisterComponent<ITestProjectComponent, TestProjectComponent>();

        // Act
        var result = _manager.TryGetComponent<ITestProjectComponent>(out var component);

        // Assert
        result.Should().BeTrue();
        component.Should().NotBeNull();
    }
}
