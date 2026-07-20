using DogSabILogger = DogSab.Platform.Core.Abstractions.Logging.ILogger;
using DogSabILoggerFactory = DogSab.Platform.Core.Abstractions.Logging.ILoggerFactory;
using MsILoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;

namespace DogSab.Platform.Core.Logging.Impl.Adapters;

/// <summary>
/// Implements the platform's <see cref="DogSabILoggerFactory"/> contract by
/// delegating creation of loggers to a wrapped Microsoft.Extensions.Logging
/// <see cref="MsILoggerFactory"/>, wrapping each created logger in a
/// <see cref="MicrosoftLoggerAdapter"/>. This is the single entry point through
/// which the rest of the platform obtains loggers — no other platform code
/// should reference <see cref="MsILoggerFactory"/> directly.
/// </summary>
public sealed class MicrosoftLoggerFactoryAdapter : DogSabILoggerFactory
{
    /// <summary>The underlying Microsoft logger factory this adapter delegates to.</summary>
    private readonly MsILoggerFactory _inner;

    /// <summary>
    /// Creates a new adapter wrapping a Microsoft.Extensions.Logging factory.
    /// </summary>
    /// <param name="inner">The underlying factory to delegate to.</param>
    public MicrosoftLoggerFactoryAdapter(MsILoggerFactory inner)
    {
        _inner = inner;
    }
    
    /// <summary>
    /// Gets a logger scoped to the given type's full name.
    /// </summary>
    /// <param name="category">The type used to derive the logger's category name.</param>
    /// <returns>A logger scoped to that category.</returns>
    public DogSabILogger GetLogger(Type category)
    {
        var msLogger = _inner.CreateLogger(category.FullName ?? category.Name);
        return new MicrosoftLoggerAdapter(msLogger);
    }

    /// <summary>
    /// Get a logger scoped to an explicit category name.
    /// </summary>
    /// <param name="category">The category name for the logger.</param>
    /// <returns>A logger scoped to that category.</returns>
    public DogSabILogger GetLogger(string category)
    {
        var msLogger = _inner.CreateLogger(category);
        return new MicrosoftLoggerAdapter(msLogger);
    }
}