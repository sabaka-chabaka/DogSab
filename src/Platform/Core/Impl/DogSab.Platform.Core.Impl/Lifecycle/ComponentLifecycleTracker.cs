using System.Collections.Concurrent;
using DogSab.Platform.Core.Abstractions.Components;
using DogSab.Platform.Core.Abstractions.Logging;

namespace DogSab.Platform.Core.Impl.Lifecycle;

/// <summary>
/// Tracks the current <see cref="ComponentLifecycleState"/> of every known component
/// and records a history of state transitions for diagnostics and troubleshooting
/// (e.g. inspecting why a component failed to reach <see cref="ComponentLifecycleState.Initialized"/>).
/// </summary>
public sealed class ComponentLifecycleTracker
{
    /// <summary>Logger used to report state transitions at debug level.</summary>
    private readonly ILogger _logger;

    /// <summary>Current lifecycle state per component interface type.</summary>
    private readonly ConcurrentDictionary<Type, ComponentLifecycleState> _currentStates = new();

    /// <summary>Ordered history of all recorded transitions, across all component types.</summary>
    private readonly List<(Type ComponentType, ComponentLifecycleState From, ComponentLifecycleState To, DateTime TimestampUtc)> _history = new();

    /// <summary>Synchronizes writes to <see cref="_history"/>, which is not thread-safe on its own.</summary>
    private readonly object _historyLock = new();

    /// <summary>
    /// Creates a new lifecycle tracker.
    /// </summary>
    /// <param name="loggerFactory">Factory used to obtain a logger scoped to this tracker.</param>
    public ComponentLifecycleTracker(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.GetLogger(typeof(ComponentLifecycleTracker));
    }

    /// <summary>
    /// Records a transition of the given component to a new lifecycle state,
    /// updating its current state and appending to the transition history.
    /// </summary>
    /// <param name="componentType">The component interface type that transitioned.</param>
    /// <param name="newState">The state the component transitioned to.</param>
    public void RecordTransition(Type componentType, ComponentLifecycleState newState)
    {
        var previousState = _currentStates.TryGetValue(componentType, out var existing)
            ? existing
            : ComponentLifecycleState.NotInitialized;

        _currentStates[componentType] = newState;

        lock (_historyLock)
        {
            _history.Add((componentType, previousState, newState, DateTime.UtcNow));
        }

        _logger.Debug(
            "Component {0} transitioned {1} -> {2}",
            componentType.FullName ?? componentType.Name,
            previousState,
            newState);
    }

    /// <summary>
    /// Returns the current lifecycle state of a component, or <see cref="ComponentLifecycleState.NotInitialized"/>
    /// if no transition has ever been recorded for it.
    /// </summary>
    /// <param name="componentType">The component interface type to query.</param>
    /// <returns>The component's current lifecycle state.</returns>
    public ComponentLifecycleState GetCurrentState(Type componentType)
    {
        return _currentStates.TryGetValue(componentType, out var state)
            ? state
            : ComponentLifecycleState.NotInitialized;
    }

    /// <summary>
    /// Returns a snapshot of the full transition history recorded so far, in the
    /// order transitions occurred, across all component types.
    /// </summary>
    /// <returns>An immutable snapshot list of recorded transitions.</returns>
    public IReadOnlyList<(Type ComponentType, ComponentLifecycleState From, ComponentLifecycleState To, DateTime TimestampUtc)> GetHistory()
    {
        lock (_historyLock)
        {
            return _history.ToArray();
        }
    }
}