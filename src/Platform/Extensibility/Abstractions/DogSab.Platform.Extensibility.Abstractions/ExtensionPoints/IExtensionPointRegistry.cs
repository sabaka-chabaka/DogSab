namespace DogSab.Platform.Extensibility.Abstractions.ExtensionPoints;

/// <summary>
/// The central registry through which the platform declares extension points
/// and plugins register implementations against them. Platform subsystems
/// declare an extension point once (typically as a static field, see
/// <see cref="ExtensionPointName{TContract}"/>) and query
/// <see cref="GetExtensions{TContract}"/> at runtime to discover every
/// implementation currently registered, regardless of which plugin — or
/// whether the platform itself — provided it.
/// </summary>
public interface IExtensionPointRegistry
{
    /// <summary>
    /// Declares a new extension point, making it discoverable by ID for plugin
    /// manifests to register against. Must be called once per extension point,
    /// typically by the platform subsystem that owns it, before any plugin
    /// attempts to register an implementation against it.
    /// </summary>
    /// <typeparam name="TContract">The interface implementations registered under this extension point must satisfy.</typeparam>
    /// <param name="extensionPoint">The extension point identity being declared.</param>
    /// <param name="area">The level at which registered implementations are shared.</param>
    void RegisterExtensionPoint<TContract>(ExtensionPointName<TContract> extensionPoint, ExtensionPointArea area)
        where TContract : class;

    /// <summary>
    /// Registers a single implementation instance against an already-declared
    /// extension point. Called by the plugin loader once it has instantiated a
    /// class named in a plugin manifest's <see cref="Manifest.ExtensionDeclaration"/>.
    /// </summary>
    /// <typeparam name="TContract">The extension point's contract type.</typeparam>
    /// <param name="extensionPoint">The extension point to register against.</param>
    /// <param name="implementation">The implementation instance to register.</param>
    void RegisterExtension<TContract>(ExtensionPointName<TContract> extensionPoint, TContract implementation)
        where TContract : class;

    /// <summary>
    /// Removes a previously registered implementation from an extension point.
    /// Used when a plugin providing the implementation is unloaded or disabled.
    /// </summary>
    /// <typeparam name="TContract">The extension point's contract type.</typeparam>
    /// <param name="extensionPoint">The extension point to unregister from.</param>
    /// <param name="implementation">The implementation instance to remove.</param>
    void UnregisterExtension<TContract>(ExtensionPointName<TContract> extensionPoint, TContract implementation)
        where TContract : class;

    /// <summary>
    /// Returns every implementation currently registered against an extension
    /// point, in registration order. Returns an empty list if the extension
    /// point has no registered implementations, or has not been declared at all.
    /// </summary>
    /// <typeparam name="TContract">The extension point's contract type.</typeparam>
    /// <param name="extensionPoint">The extension point to query.</param>
    /// <returns>A snapshot list of currently registered implementations.</returns>
    IReadOnlyList<TContract> GetExtensions<TContract>(ExtensionPointName<TContract> extensionPoint)
        where TContract : class;

    /// <summary>
    /// Checks whether an extension point has already been declared via
    /// <see cref="RegisterExtensionPoint{TContract}"/>.
    /// </summary>
    /// <param name="extensionPointId">The string identifier of the extension point to check.</param>
    /// <returns><c>true</c> if declared; otherwise <c>false</c>.</returns>
    bool IsExtensionPointDeclared(string extensionPointId);
}