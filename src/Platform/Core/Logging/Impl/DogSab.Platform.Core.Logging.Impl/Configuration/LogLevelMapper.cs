using DogSabLogLevel = DogSab.Platform.Core.Abstractions.Logging.LogLevel;
using MsLogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace DogSab.Platform.Core.Logging.Impl.Configuration;

/// <summary>
/// Converts between the platform's own <see cref="DogSabLogLevel"/> and
/// <see cref="MsLogLevel"/>. The two enums do not map one-to-one: Microsoft's
/// scale has <c>Trace</c> below Debug and <c>Critical</c> above Error, which the
/// platform's simpler five-level scale does not distinguish. Mapping to Microsoft
/// always produces an exact match; mapping from Microsoft collapses <c>Trace</c>
/// into <see cref="DogSabLogLevel.Debug"/> and <c>Critical</c> into
/// <see cref="DogSabLogLevel.Fatal"/>.
/// </summary>
public static class LogLevelMapper
{
    /// <summary>
    /// Converts a platform log level to its Microsoft.Extensions.Logging equivalent.
    /// </summary>
    /// <param name="level">The platform log level to convert.</param>
    /// <returns>The corresponding <see cref="MsLogLevel"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown for an unrecognized <paramref name="level"/>.</exception>
    public static MsLogLevel ToMicrosoft(DogSabLogLevel level)
    {
        return level switch
        {
            DogSabLogLevel.Debug => MsLogLevel.Debug,
            DogSabLogLevel.Info => MsLogLevel.Information,
            DogSabLogLevel.Warn => MsLogLevel.Warning,
            DogSabLogLevel.Error => MsLogLevel.Error,
            DogSabLogLevel.Fatal => MsLogLevel.Critical,
            _ => throw new ArgumentOutOfRangeException(nameof(level), level, "Unrecognized platform log level.")
        };
    }

    /// <summary>
    /// Converts a Microsoft.Extensions.Logging level to its closest platform equivalent.
    /// <see cref="MsLogLevel.Trace"/> collapses into <see cref="DogSabLogLevel.Debug"/>;
    /// <see cref="MsLogLevel.Critical"/> collapses into <see cref="DogSabLogLevel.Fatal"/>.
    /// </summary>
    /// <param name="level">The Microsoft log level to convert.</param>
    /// <returns>The corresponding <see cref="DogSabLogLevel"/>, or <c>null</c> if <paramref name="level"/> is <see cref="MsLogLevel.None"/>.</returns>
    public static DogSabLogLevel? ToPlatform(MsLogLevel level)
    {
        return level switch
        {
            MsLogLevel.Trace => DogSabLogLevel.Debug,
            MsLogLevel.Debug => DogSabLogLevel.Debug,
            MsLogLevel.Information => DogSabLogLevel.Info,
            MsLogLevel.Warning => DogSabLogLevel.Warn,
            MsLogLevel.Error => DogSabLogLevel.Error,
            MsLogLevel.Critical => DogSabLogLevel.Fatal,
            MsLogLevel.None => null,
            _ => throw new ArgumentOutOfRangeException(nameof(level), level, "Unrecognized Microsoft log level.")
        };
    }
}