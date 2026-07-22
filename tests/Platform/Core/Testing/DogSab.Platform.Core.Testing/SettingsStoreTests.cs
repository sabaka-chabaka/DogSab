using DogSab.Platform.Core.Abstractions.Logging;
using DogSab.Platform.Core.Abstractions.Settings;
using DogSab.Platform.Core.Settings.Impl.Paths;
using DogSab.Platform.Core.Settings.Impl.Store;
using FluentAssertions;
using Moq;

namespace DogSab.Platform.Core.Testing;

public class SettingsStoreTests : IDisposable
{
    private readonly string _tempDir;
    private readonly SettingsStoreImpl _store;
    private readonly Mock<ILoggerFactory> _loggerFactoryMock;

    public SettingsStoreTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);

        var pathResolver = new SettingsPathResolver(_tempDir);
        _loggerFactoryMock = new Mock<ILoggerFactory>();
        _loggerFactoryMock.Setup(x => x.GetLogger(It.IsAny<Type>())).Returns(new Mock<ILogger>().Object);

        _store = new SettingsStoreImpl(pathResolver, _loggerFactoryMock.Object);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
    }

    [PersistentState("test_settings.xml", Scope = SettingsScope.Project)]
    public class TestSettings
    {
        public string Value { get; set; } = "Default";
    }

    public class NonAnnotatedSettings
    {
    }

    [Fact]
    public void Load_ShouldReturnDefault_WhenFileDoesNotExist()
    {
        // Act
        var settings = _store.Load<TestSettings>();

        // Assert
        settings.Should().NotBeNull();
        settings.Value.Should().Be("Default");
    }

    [Fact]
    public void SaveAndLoad_ShouldPreserveValues()
    {
        // Arrange
        var settings = new TestSettings { Value = "Changed" };

        // Act
        _store.Save(settings);
        var loaded = _store.Load<TestSettings>();

        // Assert
        loaded.Value.Should().Be("Changed");
    }

    [Fact]
    public void Load_ShouldReturnDefault_WhenFileIsCorrupt()
    {
        // Arrange
        var pathResolver = new SettingsPathResolver(_tempDir);
        var filePath = pathResolver.ResolveFilePath("test_settings.xml", SettingsScope.Project);
        File.WriteAllText(filePath, "NOT XML");

        // Act
        var settings = _store.Load<TestSettings>();

        // Assert
        settings.Value.Should().Be("Default");
    }

    [Fact]
    public void Load_ShouldThrow_WhenTypeIsNotAnnotated()
    {
        // Act
        var act = () => _store.Load<NonAnnotatedSettings>();

        // Assert
        act.Should().Throw<InvalidOperationException>().And.Message.Should().Contain("PersistentStateAttribute");
    }
}
