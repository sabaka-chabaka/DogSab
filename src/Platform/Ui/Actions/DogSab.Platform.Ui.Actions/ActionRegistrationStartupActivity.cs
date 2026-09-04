using DogSab.Platform.Core.Abstractions.Lifecycle;
using DogSab.Platform.Core.Abstractions.Logging;
using DogSab.Platform.Extensibility.Abstractions.Attributes;
using DogSab.Platform.Ui.Actions.Abstractions;

namespace DogSab.Platform.Ui.Actions;

/// <summary>
/// Platform startup activity that walks every <see cref="AnAction"/>
/// registered against <see cref="ActionExtensionPoints.ACTION"/> and assigns
/// each a stable ID via <see cref="ActionManagerImpl.RegisterActionId"/>. IDs
/// are derived from the action's declaring type's full name, giving a stable,
/// collision-resistant default without requiring every action author to
/// invent and coordinate their own ID scheme — plugins that need a more
/// stable ID (surviving a class rename) can still register their own
/// mapping later; this activity only establishes the baseline.
/// </summary>
[Extension("core.startupActivity")]
public sealed class ActionRegistrationStartupActivity : IStartupActivity
{
    private readonly ActionManagerImpl _actionManager;
    private readonly ILoggerFactory _loggerFactory;

    public ActionRegistrationStartupActivity(ActionManagerImpl actionManager, ILoggerFactory loggerFactory)
    {
        _actionManager = actionManager;
        _loggerFactory = loggerFactory;
    }

    /// <summary>Runs after Extensibility/plugin loading has completed, so all actions plugins contribute are already registered.</summary>
    public int Order => 2100;

    public Task RunActivityAsync(CancellationToken cancellationToken)
    {
        var logger = _loggerFactory.GetLogger(typeof(ActionRegistrationStartupActivity));
        var registeredCount = 0;

        foreach (var action in _actionManager.AllActions)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var actionId = action.GetType().FullName ?? action.GetType().Name;

            try
            {
                _actionManager.RegisterActionId(actionId, action);
                registeredCount++;
            }
            catch (System.InvalidOperationException ex)
            {
                logger.Warn("Skipping duplicate action registration for id '{0}': {1}", actionId, ex.Message);
            }
        }

        logger.Info("Registered {0} action(s).", registeredCount);

        return Task.CompletedTask;
    }
}