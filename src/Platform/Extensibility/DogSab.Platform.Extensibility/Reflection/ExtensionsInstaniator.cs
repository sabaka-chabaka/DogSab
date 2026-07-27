using System;
using System.Reflection;
using DogSab.Platform.Extensibility.Diagnostics;

namespace DogSab.Platform.Extensibility.Reflection;

/// <summary>
/// Creates an instance of a plugin-declared extension class by its
/// fully-qualified name, verifying it implements the extension point's
/// declared contract before construction. The single place in the platform
/// that turns a manifest's <see cref="Abstractions.Manifest.ExtensionDeclaration.ImplementationClassName"/>
/// string into a real, usable object.
/// </summary>
public sealed class ExtensionInstantiator
{
    /// <summary>
    /// Instantiates a class by name from a specific assembly, verifying it
    /// implements the given contract type before construction.
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
        var type = assembly.GetType(fullyQualifiedTypeName, throwOnError: false);

        if (type is null)
        {
            throw new ExtensionInstantiationException(
                extensionPointId,
                fullyQualifiedTypeName,
                $"type not found in assembly '{assembly.GetName().Name}'.");
        }

        if (!typeof(TContract).IsAssignableFrom(type))
        {
            throw new ExtensionInstantiationException(
                extensionPointId,
                fullyQualifiedTypeName,
                $"does not implement required contract '{typeof(TContract).FullName}'.");
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

        return (TContract)instance;
    }
}