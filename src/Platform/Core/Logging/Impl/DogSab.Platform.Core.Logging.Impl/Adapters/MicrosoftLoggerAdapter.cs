using DogSab.Platform.Core.Logging.Impl.Configuration;
using Microsoft.Extensions.Logging;
using DogSabLogLevel = DogSab.Platform.Core.Abstractions.Logging.LogLevel;
using MsILogger = Microsoft.Extensions.Logging.ILogger;

namespace DogSab.Platform.Core.Logging.Impl.Adapters;

/// Implements the platform's <see cref="Abstractions.Logging.ILogger"/> contract
/// by delegating every call to a wrapped Microsoft.Extensions.Logging
/// <see cref="MsILogger"/> instance, translating log levels via <see cref="LogLevelMapper"/>.
/// This is the only type in the platform that constructs Microsoft log messages
/// directly — everything else in the platform logs through the abstract <c>ILogger</c>.
/// </summary>
public sealed class MicrosoftLoggerAdapter : Abstractions.Logging.ILogger
{
    /// <summary>The underlying Microsoft logger this adapter delegates to.</summary>
    private readonly MsILogger _inner;
    
    /// <summary>
    /// Creates a new adapter wrapping a Microsoft.Extensions.Logging logger.
    /// </summary>
    /// <param name="inner">The underlying logger to delegate to.</param>
    public MicrosoftLoggerAdapter(MsILogger inner)
    {
        _inner = inner;
    }

    /// <inheritdoc />
    public void Debug(string message, params object?[] args)
    {
        _inner.LogDebug(message, args);
    }

    /// <inheritdoc />
    public void Info(string message, params object?[] args)
    {
        _inner.LogInformation(message, args);
    }

    /// <inheritdoc />
    public void Warn(string message, params object?[] args)
    {
        _inner.LogWarning(message, args);
    }

    /// <inheritdoc />
    public void Error(string message, Exception? exception = null, params object?[] args)
    {
        _inner.LogError(exception, message, args);
    }

    /// <inheritdoc />
    public void Fatal(string message, Exception? exception = null, params object?[] args)
    {
        _inner.LogCritical(exception, message, args);
    }

    /// <inheritdoc />
    public bool IsEnabled(DogSabLogLevel level)
    {
        return _inner.IsEnabled(LogLevelMapper.ToMicrosoft(level));
    }
}