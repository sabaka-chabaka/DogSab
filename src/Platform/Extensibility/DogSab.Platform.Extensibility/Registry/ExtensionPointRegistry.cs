using System.Collections.Concurrent;
using DogSab.Platform.Extensibility.Abstractions.ExtensionPoints;

namespace DogSab.Platform.Extensibility.Registry;

/// <summary>
/// Internal storage for a single declared extension point: its contract type,
/// its area (application- or project-scoped), and the implementations
/// currently registered against it. Registered implementations are stored per
/// scope key: <c>null</c> for the single application-wide list shared by
/// <see cref="ExtensionPointArea.Application"/> extension points, or a
/// specific project's <see cref="Guid"/> for <see cref="ExtensionPointArea.Project"/>
/// extension points, each project getting its own independent list.
/// </summary>
internal sealed class ExtensionPointEntry
{
    private static readonly Guid ApplicationScopeKey = Guid.Empty;

    /// <summary>The contract type implementations registered against this extension point must satisfy.</summary>
    public Type ContractType { get; }

    /// <summary>Whether this extension point's registrations are shared application-wide or per-project.</summary>
    public ExtensionPointArea Area { get; }

    /// <summary>
    /// Registered implementations, keyed by scope: <c>Guid.Empty</c> for the
    /// application-wide list, or a specific project's ID for that project's list.
    /// </summary>
    private readonly ConcurrentDictionary<Guid, List<object>> _implementationsByScope = new();

    /// <summary>Guards mutation of an individual scope's list, since List&lt;T&gt; itself is not thread-safe.</summary>
    private readonly ConcurrentDictionary<Guid, object> _scopeLocks = new();

    /// <summary>
    /// Creates a new extension point entry.
    /// </summary>
    /// <param name="contractType">The contract type implementations must satisfy.</param>
    /// <param name="area">Whether registrations are shared application-wide or per-project.</param>
    public ExtensionPointEntry(Type contractType, ExtensionPointArea area)
    {
        ContractType = contractType;
        Area = area;
    }

    private static Guid ToKey(Guid? scopeKey) => scopeKey ?? ApplicationScopeKey;

    /// <summary>
    /// Registers an implementation instance under the given scope key.
    /// </summary>
    /// <param name="scopeKey">
    /// <c>null</c> to register in the application-wide list, or a project ID
    /// to register in that project's list. Should be <c>null</c> for entries
    /// with <see cref="Area"/> equal to <see cref="ExtensionPointArea.Application"/>.
    /// </param>
    /// <param name="implementation">The implementation instance to register.</param>
    public void Add(Guid? scopeKey, object implementation)
    {
        var key = ToKey(scopeKey);
        var list = _implementationsByScope.GetOrAdd(key, static _ => new List<object>());
        var lockObj = _scopeLocks.GetOrAdd(key, static _ => new object());

        lock (lockObj)
        {
            list.Add(implementation);
        }
    }

    /// <summary>
    /// Removes a previously registered implementation instance from the given scope's list.
    /// </summary>
    /// <param name="scopeKey">The scope key the implementation was registered under.</param>
    /// <param name="implementation">The implementation instance to remove.</param>
    public void Remove(Guid? scopeKey, object implementation)
    {
        var key = ToKey(scopeKey);
        if (!_implementationsByScope.TryGetValue(key, out var list))
        {
            return;
        }
        
        var lockObj = _scopeLocks.GetOrAdd(key, static _ => new object());

        lock (lockObj)
        {
            list.Remove(implementation);
        }
    }

    /// <summary>
    /// Returns a snapshot of every implementation currently registered under
    /// the given scope, in registration order. Returns an empty list if no
    /// implementations are registered for that scope.
    /// </summary>
    /// <param name="scopeKey">The scope key to query.</param>
    /// <returns>A snapshot list of registered implementations for that scope.</returns>
    public IReadOnlyList<object> GetAll(Guid? scopeKey)
    {
        var key = ToKey(scopeKey);
        if (!_implementationsByScope.TryGetValue(key, out var list))
        {
            return [];
        }

        var lockObj = _scopeLocks.GetOrAdd(key, static _ => new object());

        lock (lockObj)
        {
            return [.. list];
        }
    }

    /// <summary>
    /// Removes an entire project's scope and all its registered implementations.
    /// Called when a project is closed, so extensions registered specifically
    /// for it do not linger in memory.
    /// </summary>
    /// <param name="projectId">The ID of the closed project's scope to remove.</param>
    public void RemoveScope(Guid projectId)
    {
        _implementationsByScope.TryRemove(projectId, out _);
        _scopeLocks.TryRemove(projectId, out _);
    }
}