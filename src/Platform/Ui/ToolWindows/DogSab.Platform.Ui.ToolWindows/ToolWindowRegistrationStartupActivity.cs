using DogSab.Platform.Core.Abstractions.Lifecycle;
using DogSab.Platform.Core.Abstractions.Logging;

namespace DogSab.Platform.Ui.ToolWindows;

/// <summary>
/// Platform startup activity that logs which tool window factories are
/// registered, mirroring the diagnostics pattern used throughout the
/// platform. Does not eagerly open any tool windows — that remains a
/// user-driven or persisted-layout concern for <c>Ui.Shell</c>, not something
/// startup itself decides.
/// </summary>
public sealed class ToolWindowRegistrationStartupActivity : IStartupActivity
{
    private readonly ToolWindowManagerImpl _toolWindowManager;
    private readonly ILoggerFactory _loggerFactory;

    public ToolWindowRegistrationStartupActivity(ToolWindowManagerImpl toolWindowManager, ILoggerFactory loggerFactory)
    {
        _toolWindowManager = toolWindowManager;
        _loggerFactory = loggerFactory;
    }

    public int Order => 2050;

    public Task RunActivityAsync(CancellationToken cancellationToken)
    {
        var logger = _loggerFactory.GetLogger(typeof(ToolWindowRegistrationStartupActivity));
        var factories = _toolWindowManager.AllFactories;

        logger.Info("{0} tool window factory(s) registered.", factories.Count);

        foreach (var factory in factories)
        {
            logger.Debug("Tool window '{0}' ({1})", factory.ToolWindowId, factory.Title);
        }

        return Task.CompletedTask;
    }
}