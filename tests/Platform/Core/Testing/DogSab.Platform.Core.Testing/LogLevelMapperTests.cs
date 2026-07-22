using DogSabLogLevel = DogSab.Platform.Core.Abstractions.Logging.LogLevel;
using MsLogLevel = Microsoft.Extensions.Logging.LogLevel;
using DogSab.Platform.Core.Logging.Impl.Configuration;
using FluentAssertions;

namespace DogSab.Platform.Core.Testing;

public class LogLevelMapperTests
{
    [Theory]
    [InlineData(DogSabLogLevel.Debug, MsLogLevel.Debug)]
    [InlineData(DogSabLogLevel.Info, MsLogLevel.Information)]
    [InlineData(DogSabLogLevel.Warn, MsLogLevel.Warning)]
    [InlineData(DogSabLogLevel.Error, MsLogLevel.Error)]
    [InlineData(DogSabLogLevel.Fatal, MsLogLevel.Critical)]
    public void ToMicrosoft_ShouldMapCorrectly(DogSabLogLevel platformLevel, MsLogLevel expectedMsLevel)
    {
        LogLevelMapper.ToMicrosoft(platformLevel).Should().Be(expectedMsLevel);
    }

    [Theory]
    [InlineData(MsLogLevel.Trace, DogSabLogLevel.Debug)]
    [InlineData(MsLogLevel.Debug, DogSabLogLevel.Debug)]
    [InlineData(MsLogLevel.Information, DogSabLogLevel.Info)]
    [InlineData(MsLogLevel.Warning, DogSabLogLevel.Warn)]
    [InlineData(MsLogLevel.Error, DogSabLogLevel.Error)]
    [InlineData(MsLogLevel.Critical, DogSabLogLevel.Fatal)]
    public void ToPlatform_ShouldMapCorrectly(MsLogLevel msLevel, DogSabLogLevel expectedPlatformLevel)
    {
        LogLevelMapper.ToPlatform(msLevel).Should().Be(expectedPlatformLevel);
    }

    [Fact]
    public void ToPlatform_ShouldReturnNull_ForNone()
    {
        LogLevelMapper.ToPlatform(MsLogLevel.None).Should().BeNull();
    }
}
