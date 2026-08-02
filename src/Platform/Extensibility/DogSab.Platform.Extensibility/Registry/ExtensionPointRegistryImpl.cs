using System.Collections.Concurrent;
using DogSab.Platform.Core.Abstractions.Project;
using DogSab.Platform.Extensibility.Abstractions.ExtensionPoints;
using DogSab.Platform.Extensibility.Diagnostics;

namespace DogSab.Platform.Extensibility.Registry;

/// <summary>
/// Default implementation of <see cref="IExtensionPointRegistry"/>.
/// Stores one <see cref="ExtensionPointEntry"/> per declared extension point,
/// keyed by the extension point's string ID. For project-scoped extension
/// points, the active project's scope is resolved automatically via
/// <see cref="ICurrentProjectAccessor"/>, so callers never need to pass a
/// project ID explicitly when registering or querying extensions.
/// </summary>
public sealed class ExtensionPointRegistryImpl : IExtensionPointRegistry
{
    /// <summary>Declared extension point entries, keyed by their string ID.</summary>
    private readonly ConcurrentDictionary<string, ExtensionPointEntry> _entriesById = new();

    /// <summary>Used to resolve the active project's scope for project-area extension points.</summary>
    private readonly ICurrentProjectAccessor _currentProjectAccessor;

    /// <summary>
    /// Creates a new extension point registry.
    /// </summary>
    /// <param name="currentProjectAccessor">Accessor used to resolve the active project's scope.</param>
    public ExtensionPointRegistryImpl(ICurrentProjectAccessor currentProjectAccessor)
    {
        _currentProjectAccessor = currentProjectAccessor;
    }
    
    /// <summary>
    /// Declares a new extension point, making it discoverable by ID for plugin
    /// manifests to register against.
    /// </summary>
    /// <typeparam name="TContract">The interface implementations registered under this extension point must satisfy.</typeparam>
    /// <param name="extensionPoint">The extension point identity being declared.</param>
    /// <param name="area">The level at which registered implementations are shared.</param>
    /// <exception cref="DuplicateExtensionPointException">
    /// Thrown if an extension point with the same <see cref="ExtensionPointName{TContract}.Id"/>
    /// has already been declared, whether with the same or a different contract type.
    /// </exception>
    public void RegisterExtensionPoint<TContract>(ExtensionPointName<TContract> extensionPoint, ExtensionPointArea area)
        where TContract : class
    {
        var newEntry = new ExtensionPointEntry(typeof(TContract), area);

        var addedEntry = _entriesById.GetOrAdd(extensionPoint.Id, newEntry);

        if (!ReferenceEquals(addedEntry, newEntry))
        {
            // Another registration already exists under this ID — always a
            // conflict, since RegisterExtensionPoint must only ever be called
            // once per extension point.
            throw new DuplicateExtensionPointException(extensionPoint.Id, addedEntry.ContractType, typeof(TContract));
        }
    }
    
    /// <summary>
    /// Registers a single implementation instance against an already-declared
    /// extension point.
    /// </summary>
    /// <typeparam name="TContract">The extension point's contract type.</typeparam>
    /// <param name="extensionPoint">The extension point to register against.</param>
    /// <param name="implementation">The implementation instance to register.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if <paramref name="extensionPoint"/> has not been declared via
    /// <see cref="RegisterExtensionPoint{TContract}"/>, or if it is project-scoped
    /// and no project is currently active.
    /// </exception>
    public void RegisterExtension<TContract>(ExtensionPointName<TContract> extensionPoint, TContract implementation)
        where TContract : class
    {
        var entry = GetDeclaredEntry(extensionPoint);
        var scopeKey = ResolveScopeKey(entry.Area);

        entry.Add(scopeKey, implementation);
    }
    
    /// <summary>
    /// Removes a previously registered implementation from an extension point.
    /// </summary>
    /// <typeparam name="TContract">The extension point's contract type.</typeparam>
    /// <param name="extensionPoint">The extension point to unregister from.</param>
    /// <param name="implementation">The implementation instance to remove.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if <paramref name="extensionPoint"/> has not been declared, or if
    /// it is project-scoped and no project is currently active.
    /// </exception>
    public void UnregisterExtension<TContract>(ExtensionPointName<TContract> extensionPoint, TContract implementation)
        where TContract : class
    {
        var entry = GetDeclaredEntry(extensionPoint);
        var scopeKey = ResolveScopeKey(entry.Area);

        entry.Remove(scopeKey, implementation);
    }

    /// <summary>
    /// Returns every implementation currently registered against an extension
    /// point, for the currently active scope, in registration order.
    /// </summary>
    /// <typeparam name="TContract">The extension point's contract type.</typeparam>
    /// <param name="extensionPoint">The extension point to query.</param>
    /// <returns>A snapshot list of currently registered implementations.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if <paramref name="extensionPoint"/> has not been declared, or if
    /// it is project-scoped and no project is currently active.
    /// </exception>
    public IReadOnlyList<TContract> GetExtensions<TContract>(ExtensionPointName<TContract> extensionPoint)
        where TContract : class
    {
        var entry = GetDeclaredEntry(extensionPoint);
        var scopeKey = ResolveScopeKey(entry.Area);

        var rawImplementations = entry.GetAll(scopeKey);
        var typedImplementations = new List<TContract>(rawImplementations.Count);

        foreach (var raw in rawImplementations)
        {
            typedImplementations.Add((TContract)raw);
        }

        return typedImplementations;
    }

    /// <summary>
    /// Checks whether an extension point has already been declared.
    /// </summary>
    /// <param name="extensionPointId">The string identifier of the extension point to check.</param>
    /// <returns><c>true</c> if declared; otherwise <c>false</c>.</returns>
    public bool IsExtensionPointDeclared(string extensionPointId)
    {
        return _entriesById.ContainsKey(extensionPointId);
    }

    /// <summary>
    /// Removes an entire project's registered extensions across every declared
    /// project-scoped extension point. Called when a project is closed, so
    /// extensions registered specifically for it do not linger in memory.
    /// Not part of <see cref="IExtensionPointRegistry"/> itself, since it is a
    /// platform-internal lifecycle operation rather than something plugins
    /// should ever call.
    /// </summary>
    /// <param name="projectId">The ID of the closed project's scope to remove.</param>
    internal void RemoveProjectScope(Guid projectId)
    {
        foreach (var entry in _entriesById.Values)
        {
            if (entry.Area == ExtensionPointArea.Project)
            {
                entry.RemoveScope(projectId);
            }
        }
    }

    /// <summary>
    /// Looks up the declared entry for an extension point, verifying its
    /// contract type matches <typeparamref name="TContract"/>.
    /// </summary>
    /// <typeparam name="TContract">The expected contract type.</typeparam>
    /// <param name="extensionPoint">The extension point to look up.</param>
    /// <returns>The declared entry.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if no entry is declared under <paramref name="extensionPoint"/>'s ID.
    /// </exception>
    private ExtensionPointEntry GetDeclaredEntry<TContract>(ExtensionPointName<TContract> extensionPoint)
        where TContract : class
    {
        if (!_entriesById.TryGetValue(extensionPoint.Id, out var entry))
        {
            throw new InvalidOperationException(
                $"Extension point '{extensionPoint.Id}' has not been declared. " +
                $"Call {nameof(RegisterExtensionPoint)} before registering or querying extensions against it.");
        }

        return entry;
    }

    /// <summary>
    /// Resolves the scope key under which to store/look up implementations for
    /// a given extension point's area: <c>null</c> for application-scoped
    /// extension points, or the currently active project's ID for
    /// project-scoped ones.
    /// </summary>
    /// <param name="area">The extension point's declared area.</param>
    /// <returns>The resolved scope key.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if <paramref name="area"/> is <see cref="ExtensionPointArea.Project"/>
    /// but no project is currently active according to <see cref="ICurrentProjectAccessor"/>.
    /// </exception>
    private Guid? ResolveScopeKey(ExtensionPointArea area)
    {
        if (area == ExtensionPointArea.Application)
        {
            return null;
        }

        return _currentProjectAccessor.CurrentProjectId
            ?? throw new InvalidOperationException(
                "Attempted to access a project-scoped extension point, but no project is currently active. " +
                "Ensure this code runs within an ICurrentProjectAccessor.EnterProjectScope(...) block.");
    }
    
    /// <summary>
    /// A single row of the extension point registry's diagnostics snapshot,
    /// summarizing one declared extension point without exposing the internal
    /// <see cref="ExtensionPointEntry"/> storage itself.
    /// </summary>
    internal readonly record struct ExtensionPointDiagnosticsRow(
        string ExtensionPointId,
        ExtensionPointArea Area,
        int ApplicationScopeImplementationCount);

    /// <summary>
    /// Gets a diagnostic snapshot of every currently declared extension point,
    /// summarizing each as its ID, area, and (for application-scoped points) how
    /// many implementations are registered. Project-scoped points report 0 here
    /// regardless of actual per-project registrations, since there is no single
    /// "current project" during a global diagnostics pass.
    /// </summary>
    /// <returns>A read-only list of summarized extension point rows.</returns>
    internal IReadOnlyList<ExtensionPointDiagnosticsRow> GetDiagnosticsSnapshot()
    {
        var result = new List<ExtensionPointDiagnosticsRow>();

        foreach (var (id, entry) in _entriesById)
        {
            var applicationCount = entry.Area == ExtensionPointArea.Application
                ? entry.GetAll(null).Count
                : 0;

            result.Add(new ExtensionPointDiagnosticsRow(id, entry.Area, applicationCount));
        }

        return result;
    }
    
    /// <inheritdoc />
    public void RegisterExtensionUntyped(string extensionPointId, object implementation)
    {
        var entry = GetDeclaredEntryUntyped(extensionPointId);

        if (!entry.ContractType.IsInstanceOfType(implementation))
        {
            throw new InvalidOperationException(
                $"Cannot register instance of type '{implementation.GetType().FullName}' against extension point " +
                $"'{extensionPointId}': it does not implement the required contract '{entry.ContractType.FullName}'.");
        }

        var scopeKey = ResolveScopeKey(entry.Area);
        entry.Add(scopeKey, implementation);
    }

    /// <inheritdoc />
    public void UnregisterExtensionUntyped(string extensionPointId, object implementation)
    {
        var entry = GetDeclaredEntryUntyped(extensionPointId);
        var scopeKey = ResolveScopeKey(entry.Area);

        entry.Remove(scopeKey, implementation);
    }

    /// <summary>
    /// Looks up a declared entry by its string ID alone, without verifying
    /// against any particular compile-time contract type. Used by the untyped
    /// registration path, where the contract type is only known at runtime.
    /// </summary>
    /// <param name="extensionPointId">The extension point ID to look up.</param>
    /// <returns>The declared entry.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no entry is declared under this ID.</exception>
    private ExtensionPointEntry GetDeclaredEntryUntyped(string extensionPointId)
    {
        if (!_entriesById.TryGetValue(extensionPointId, out var entry))
        {
            throw new InvalidOperationException(
                $"Extension point '{extensionPointId}' has not been declared. " +
                $"Call {nameof(RegisterExtensionPoint)} before registering or querying extensions against it.");
        }

        return entry;
    }
    
    /// <summary>
    /// Returns the declared contract type for an extension point by its string ID.
    /// </summary>
    /// <param name="extensionPointId">The extension point ID to look up.</param>
    /// <returns>The declared contract type.</returns>
    internal Type GetContractType(string extensionPointId) => GetDeclaredEntryUntyped(extensionPointId).ContractType;
}