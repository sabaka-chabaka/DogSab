using DogSab.Platform.Core.Logging.Impl.Configuration;
using FluentAssertions;

namespace DogSab.Platform.Core.Testing;

public class LogFilePathResolverTests
{
    [Fact]
    public void GetLogsDirectory_ShouldReturnPathAndCreateDirectory()
    {
        // Act
        var logsDir = LogFilePathResolver.GetLogsDirectory();

        // Assert
        logsDir.Should().NotBeNullOrEmpty();
        logsDir.Should().Contain("DogSab");
        logsDir.Should().Contain("logs");
        Directory.Exists(logsDir).Should().BeTrue();
    }

    [Fact]
    public void GetCurrentLogFilePath_ShouldReturnPathInsideLogsDirectory()
    {
        // Act
        var logFile = LogFilePathResolver.GetCurrentLogFilePath();
        var logsDir = LogFilePathResolver.GetLogsDirectory();

        // Assert
        logFile.Should().StartWith(logsDir);
        logFile.Should().EndWith("dogsab.log");
    }
}
