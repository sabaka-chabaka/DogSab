using DogSab.Platform.Core.Logging.Impl.Adapters;
using DogSab.Platform.Core.Logging.Impl.Configuration;
using DogSab.Platform.Core.Logging.Impl.Providers;
using Microsoft.Extensions.Logging;
using DogSabILoggerFactory = DogSab.Platform.Core.Abstractions.Logging.ILoggerFactory;

namespace DogSab.Platform.Core.Logging.Impl.Bootstrap;

/// <summary>
/// Assembles the platform's logging subsystem: builds a Microsoft.Extensions.Logging
/// <see cref="ILoggerFactory"/> configured with the rolling file provider (always
/// enabled) and the console provider (enabled only when <see cref="LoggingOptions.ConsoleEnabled"/>
/// is set), then wraps it in a <see cref="MicrosoftLoggerFactoryAdapter"/> so the
/// rest of the platform receives the abstract <see cref="DogSabILoggerFactory"/>
/// contract it depends on. Called exactly once, very early during application
/// startup, before any other subsystem requests a logger.
/// </summary>
public static class LoggingBootstrapper
{
    /// <summary>
    /// Builds the platform's logger factory using options resolved from the
    /// environment (see <see cref="LoggingOptions.FromEnvironment"/>).
    /// </summary>
    /// <returns>A ready-to-use <see cref="DogSabILoggerFactory"/> for the rest of the platform.</returns>
    public static DogSabILoggerFactory Build()
    {
        return Build(LoggingOptions.FromEnvironment());
    }

    /// <summary>
    /// Builds the platform's logger factory using explicitly provided options.
    /// </summary>
    /// <param name="options">The logging configuration to apply.</param>
    /// <returns>A ready-to-use <see cref="DogSabILoggerFactory"/> for the rest of the platform.</returns>
    public static DogSabILoggerFactory Build(LoggingOptions options)
    {
        var msFactory = LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(LogLevelMapper.ToMicrosoft(options.MinimumLevel));
            builder.AddProvider(new RollingFileLoggerProvider(options));

            if (options.ConsoleEnabled)
            {
                builder.AddConsole();
            }
        });

        return new MicrosoftLoggerFactoryAdapter(msFactory);
    }
}