using DogSab.Platform.Core.Abstractions.Logging;
using DogSab.Platform.Core.Application.ProjectLifecycle;
using DogSab.Platform.Core.Impl.Services;
using FluentAssertions;
using Moq;

namespace DogSab.Platform.Core.Testing;

public class ProjectSessionManagerTests : IDisposable
{
    private readonly Mock<ILoggerFactory> _loggerFactoryMock;
    private readonly Mock<ILogger> _loggerMock;
    private readonly ServiceContainerImpl _container;
    private readonly ProjectSessionManager _manager;
    private readonly string _tempDir;

    public ProjectSessionManagerTests()
    {
        _loggerFactoryMock = new Mock<ILoggerFactory>();
        _loggerMock = new Mock<ILogger>();
        _loggerFactoryMock.Setup(f => f.GetLogger(It.IsAny<Type>())).Returns(_loggerMock.Object);

        var registry = new ServiceRegistry();
        var detector = new ServiceCircularDependencyDetector();
        var activator = new ServiceActivator(detector);
        _container = new ServiceContainerImpl(registry, activator);

        _manager = new ProjectSessionManager(_container, _loggerFactoryMock.Object);
        _tempDir = Path.Combine(Path.GetTempPath(), "DogSabTests_" + Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
    }

    [Fact]
    public void OpenProject_ShouldCreateSession_WhenDirectoryExists()
    {
        // Act
        var session = _manager.OpenProject(_tempDir);

        // Assert
        session.Should().NotBeNull();
        session.ProjectRootDirectory.Should().Be(_tempDir);
        _manager.OpenSessions.Should().Contain(session);
    }

    [Fact]
    public void OpenProject_ShouldThrow_WhenDirectoryDoesNotExist()
    {
        // Arrange
        var nonExistent = Path.Combine(_tempDir, "nonexistent");

        // Act
        var act = () => _manager.OpenProject(nonExistent);

        // Assert
        act.Should().Throw<ProjectOpenException>()
            .And.ProjectRootDirectory.Should().Be(nonExistent);
    }

    [Fact]
    public void CloseProject_ShouldRemoveSession()
    {
        // Arrange
        var session = _manager.OpenProject(_tempDir);
        var id = session.ProjectId;

        // Act
        var result = _manager.CloseProject(id);

        // Assert
        result.Should().BeTrue();
        _manager.OpenSessions.Should().NotContain(session);
    }

    [Fact]
    public void CloseAllProjects_ShouldClearSessions()
    {
        // Arrange
        var tempDir2 = Path.Combine(Path.GetTempPath(), "DogSabTests_" + Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir2);
        try
        {
            _manager.OpenProject(_tempDir);
            _manager.OpenProject(tempDir2);
            _manager.OpenSessions.Should().HaveCount(2);

            // Act
            _manager.CloseAllProjects();

            // Assert
            _manager.OpenSessions.Should().BeEmpty();
        }
        finally
        {
            if (Directory.Exists(tempDir2)) Directory.Delete(tempDir2, true);
        }
    }
}
