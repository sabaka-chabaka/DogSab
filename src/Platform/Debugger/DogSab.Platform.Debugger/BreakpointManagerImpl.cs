using System.Collections.Concurrent;
using DogSab.Platform.Debugger.Abstractions;

namespace DogSab.Platform.Debugger;

/// <summary>
/// Holds every breakpoint the user has set for the current session, keyed
/// by <see cref="BreakpointId"/>, and additionally indexed by file path
/// for fast lookup of "which breakpoints exist in this file" — the query
/// an editor's gutter rendering needs on every visible file, so a linear
/// scan of every breakpoint on every render would be wasteful once a
/// project accumulates more than a handful of breakpoints.
/// </summary>
public sealed class BreakpointManagerImpl
{
    /// <summary>
    /// Every currently set breakpoint, keyed by its ID.
    /// </summary>
    private readonly ConcurrentDictionary<BreakpointId, BreakpointImpl> _breakpointsById = new();

    /// <summary>
    /// Breakpoint IDs grouped by file path, maintained alongside
    /// <see cref="_breakpointsById"/> to answer "breakpoints in this file"
    /// queries without scanning every breakpoint.
    /// </summary>
    private readonly ConcurrentDictionary<string, HashSet<BreakpointId>> _breakpointIdsByFilePath = new();

    /// <summary>
    /// Raised whenever a breakpoint is added, removed, or toggled, so
    /// subscribed editor gutters can refresh their displayed markers.
    /// </summary>
    public event Action? BreakpointsChanged;

    /// <summary>
    /// Sets a new breakpoint at a given file and line.
    /// </summary>
    /// <param name="filePath">
    /// The file path to set the breakpoint in.
    /// </param>
    /// <param name="lineNumber">
    /// The one-based line number to set the breakpoint at.
    /// </param>
    /// <param name="conditionExpression">
    /// An optional condition expression, or <c>null</c> for an
    /// unconditional breakpoint.
    /// </param>
    /// <returns>
    /// The newly created breakpoint.
    /// </returns>
    public IBreakpoint AddBreakpoint(string filePath, int lineNumber, string? conditionExpression = null)
    {
        var breakpoint = new BreakpointImpl(BreakpointId.NewId(), filePath, lineNumber, conditionExpression);

        _breakpointsById[breakpoint.Id] = breakpoint;

        var idsForFile = _breakpointIdsByFilePath.GetOrAdd(filePath, static _ => new HashSet<BreakpointId>());
        lock (idsForFile)
        {
            idsForFile.Add(breakpoint.Id);
        }

        BreakpointsChanged?.Invoke();

        return breakpoint;
    }

    /// <summary>
    /// Removes a breakpoint by its ID.
    /// </summary>
    /// <param name="id">
    /// The identifier of the breakpoint to remove.
    /// </param>
    public void RemoveBreakpoint(BreakpointId id)
    {
        if (!_breakpointsById.TryRemove(id, out var breakpoint))
        {
            return;
        }

        if (_breakpointIdsByFilePath.TryGetValue(breakpoint.FilePath, out var idsForFile))
        {
            lock (idsForFile)
            {
                idsForFile.Remove(id);
            }
        }

        BreakpointsChanged?.Invoke();
    }

    /// <summary>
    /// Toggles a breakpoint's enabled state.
    /// </summary>
    /// <param name="id">
    /// The identifier of the breakpoint to toggle.
    /// </param>
    /// <param name="isEnabled">
    /// The new enabled state.
    /// </param>
    public void SetEnabled(BreakpointId id, bool isEnabled)
    {
        if (_breakpointsById.TryGetValue(id, out var breakpoint))
        {
            breakpoint.IsEnabled = isEnabled;
            BreakpointsChanged?.Invoke();
        }
    }

    /// <summary>
    /// Returns every breakpoint currently set in a given file, for
    /// rendering gutter markers.
    /// </summary>
    /// <param name="filePath">
    /// The file path to query breakpoints for.
    /// </param>
    /// <returns>
    /// The breakpoints set in this file, in no particular guaranteed order.
    /// </returns>
    public IReadOnlyList<IBreakpoint> GetBreakpointsForFile(string filePath)
    {
        if (!_breakpointIdsByFilePath.TryGetValue(filePath, out var idsForFile))
        {
            return Array.Empty<IBreakpoint>();
        }

        lock (idsForFile)
        {
            return idsForFile
                .Select(id => _breakpointsById[id])
                .Cast<IBreakpoint>()
                .ToList();
        }
    }

    /// <summary>
    /// Every currently set breakpoint, across every file.
    /// </summary>
    public IReadOnlyList<IBreakpoint> AllBreakpoints => _breakpointsById.Values.Cast<IBreakpoint>().ToList();

    /// <summary>
    /// Default, mutable-internally implementation of <see cref="IBreakpoint"/>.
    /// </summary>
    private sealed class BreakpointImpl : IBreakpoint
    {
        /// <inheritdoc />
        public BreakpointId Id { get; }

        /// <inheritdoc />
        public string FilePath { get; }

        /// <inheritdoc />
        public int LineNumber { get; }

        /// <inheritdoc />
        public bool IsEnabled { get; set; } = true;

        /// <inheritdoc />
        public string? ConditionExpression { get; }

        /// <summary>
        /// Creates a new breakpoint.
        /// </summary>
        /// <param name="id">
        /// The breakpoint's stable identifier.
        /// </param>
        /// <param name="filePath">
        /// The file path this breakpoint is set in.
        /// </param>
        /// <param name="lineNumber">
        /// The one-based line number this breakpoint is set at.
        /// </param>
        /// <param name="conditionExpression">
        /// An optional condition expression.
        /// </param>
        public BreakpointImpl(BreakpointId id, string filePath, int lineNumber, string? conditionExpression)
        {
            Id = id;
            FilePath = filePath;
            LineNumber = lineNumber;
            ConditionExpression = conditionExpression;
        }
    }
}