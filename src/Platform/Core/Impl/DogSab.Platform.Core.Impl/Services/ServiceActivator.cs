using DogSab.Platform.Core.Abstractions.Services;

namespace DogSab.Platform.Core.Impl.Services;

/// <summary>
/// Creates service instances via constructor injection, resolving each constructor
/// parameter through the owning <see cref="IServiceContainer"/>.
/// </summary>
public sealed class ServiceActivator
{
    /// <summary>Detects and reports circular construction chains during activation.</summary>
    private readonly ServiceCircularDependencyDetector _circularDependencyDetector = new();
    
    /// <summary>
    /// Creates a new activator.
    /// </summary>
    /// <param name="circularDependencyDetector">Detector used to guard against constructor cycles.</param>
    public ServiceActivator(ServiceCircularDependencyDetector circularDependencyDetector)
    {
        _circularDependencyDetector = circularDependencyDetector;
    }
    
    /// <summary>
    /// Constructs a new instance of the given implementation type, resolving
    /// its constructor dependencies from the provided container.
    /// </summary>
    /// <param name="registration">The registration describing the type to construct.</param>
    /// <param name="container">The container used to resolve constructor parameters.</param>
    /// <returns>The newly constructed service instance.</returns>
    public object CreateInstance(ServiceRegistration registration, IServiceContainer container)
    {
        _circularDependencyDetector.Enter(registration.ServiceType);

        try
        {
            var constructor = SelectConstructor(registration.ImplementationType);
            var parameters = constructor.GetParameters();

            if (parameters.Length == 0)
            {
                return Activator.CreateInstance(registration.ImplementationType)!;
            }

            var arguments = parameters
                .Select(p => container.GetService(p.ParameterType))
                .ToArray();

            return constructor.Invoke(arguments);
        }
        finally
        {
            _circularDependencyDetector.Exit(registration.ServiceType);
        }
    }

    /// <summary>
    /// Selects the constructor to use for activation: the single public constructor
    /// with the most parameters, so optional dependencies can be declared via
    /// multiple constructor overloads if needed.
    /// </summary>
    /// <param name="implementationType">The type whose constructor should be selected.</param>
    /// <returns>The chosen constructor.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if <paramref name="implementationType"/> declares no public constructors.
    /// </exception>
    private static System.Reflection.ConstructorInfo SelectConstructor(Type implementationType)
    {
        var constructors = implementationType.GetConstructors();

        if (constructors.Length == 0)
        {
            throw new InvalidOperationException(
                $"Type '{implementationType.FullName}' has no public constructors and cannot be activated as a service.");
        }

        return constructors.OrderByDescending(c => c.GetParameters().Length).First();
    }
}