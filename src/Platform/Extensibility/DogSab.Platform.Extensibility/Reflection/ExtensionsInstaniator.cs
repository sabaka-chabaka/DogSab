using System.Reflection;
using DogSab.Platform.Extensibility.Diagnostics;
using DogSab.Platform.Extensibility.Registry;

namespace DogSab.Platform.Extensibility.Reflection;

/// <summary>
/// Creates an instance of an extension class by its fully-qualified name,
/// verifying it implements the target extension point's contract before
/// construction. Offers two paths: a compile-time generic path
/// (<see cref="Instantiate{TContract}"/>) for platform code that already
/// knows the contract type via a compile-time
/// <see cref="ExtensionPoints.ExtensionPointName{TContract}"/> reference, and
/// an untyped runtime path (<see cref="InstantiateUntyped"/>) for code like
/// the plugin loader, which only knows an extension point's contract type by
/// looking it up from its string ID at runtime.
/// </summary>
public sealed class ExtensionInstantiator
{
    /// <summary>Used by <see cref="ResolveContractType"/> to look up a contract type from an extension point's string ID.</summary>
    private readonly ExtensionPointRegistryImpl _registry;

    /// <summary>
    /// Creates a new extension instantiator.
    /// </summary>
    /// <param name="registry">The registry used to resolve contract types for extension points looked up by string ID.</param>
    public ExtensionInstantiator(ExtensionPointRegistryImpl registry)
    {
        _registry = registry;
    }

    /// <summary>
    /// Instantiates a class by name from a specific assembly, verifying it
    /// implements the given contract type before construction. Use this
    /// overload when the contract type is known at compile time (e.g. from a
    /// platform-declared <see cref="ExtensionPoints.ExtensionPointName{TContract}"/>).
    /// </summary>
    /// <typeparam name="TContract">The contract type the instantiated class must implement.</typeparam>
    /// <param name="assembly">The assembly the implementation class should be found in.</param>
    /// <param name="extensionPointId">The extension point ID this instantiation is for, used only for error reporting.</param>
    /// <param name="fullyQualifiedTypeName">The fully-qualified name of the class to instantiate.</param>
    /// <returns>The newly constructed instance, typed as <typeparamref name="TContract"/>.</returns>
    /// <exception cref="ExtensionInstantiationException">
    /// Thrown if the type cannot be found in <paramref name="assembly"/>, does
    /// not implement <typeparamref name="TContract"/>, has no accessible
    /// parameterless constructor, or throws during construction.
    /// </exception>
    public TContract Instantiate<TContract>(Assembly assembly, string extensionPointId, string fullyQualifiedTypeName)
        where TContract : class
    {
        return (TContract)InstantiateUntyped(assembly, extensionPointId, fullyQualifiedTypeName, typeof(TContract));
    }

    /// <summary>
    /// Resolves the declared contract type for an extension point by its
    /// string ID. Needed by callers — like the plugin loader — that only know
    /// the extension point at runtime (from a plugin manifest) and therefore
    /// cannot supply a compile-time <c>TContract</c> generic argument the way
    /// <see cref="Instantiate{TContract}"/> requires.
    /// </summary>
    /// <param name="extensionPointId">The extension point's string ID.</param>
    /// <returns>The extension point's declared contract type.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no extension point is declared under this ID.</exception>
    public Type ResolveContractType(string extensionPointId)
    {
        return _registry.GetContractType(extensionPointId);
    }

    /// <summary>
    /// Instantiates a class by name from a specific assembly, verifying it
    /// implements the given contract type at runtime rather than via a
    /// compile-time generic parameter. This is the underlying implementation
    /// both <see cref="Instantiate{TContract}"/> and the plugin loader use.
    /// </summary>
    /// <param name="assembly">The assembly the implementation class should be found in.</param>
    /// <param name="extensionPointId">The extension point ID this instantiation is for, used only for error reporting.</param>
    /// <param name="fullyQualifiedTypeName">The fully-qualified name of the class to instantiate.</param>
    /// <param name="contractType">The contract type the instantiated class must implement.</param>
    /// <returns>The newly constructed instance.</returns>
    /// <exception cref="ExtensionInstantiationException">
    /// Thrown if the type cannot be found in <paramref name="assembly"/>, does
    /// not implement <paramref name="contractType"/>, has no accessible
    /// parameterless constructor, or throws during construction.
    /// </exception>
    public object InstantiateUntyped(Assembly assembly, string extensionPointId, string fullyQualifiedTypeName, Type contractType)
    {
        var type = assembly.GetType(fullyQualifiedTypeName, throwOnError: false);

        if (type is null)
        {
            throw new ExtensionInstantiationException(
                extensionPointId,
                fullyQualifiedTypeName,
                $"type not found in assembly '{assembly.GetName().Name}'.");
        }

        if (!contractType.IsAssignableFrom(type))
        {
            throw new ExtensionInstantiationException(
                extensionPointId,
                fullyQualifiedTypeName,
                $"does not implement required contract '{contractType.FullName}'.");
        }

        object instance;

        try
        {
            instance = Activator.CreateInstance(type)
                ?? throw new ExtensionInstantiationException(
                    extensionPointId,
                    fullyQualifiedTypeName,
                    "Activator.CreateInstance returned null.");
        }
        catch (MissingMethodException ex)
        {
            throw new ExtensionInstantiationException(
                extensionPointId,
                fullyQualifiedTypeName,
                "type has no accessible public parameterless constructor.",
                ex);
        }
        catch (TargetInvocationException ex) when (ex.InnerException is not null)
        {
            throw new ExtensionInstantiationException(
                extensionPointId,
                fullyQualifiedTypeName,
                $"constructor threw: {ex.InnerException.Message}",
                ex.InnerException);
        }

        return instance;
    }
}