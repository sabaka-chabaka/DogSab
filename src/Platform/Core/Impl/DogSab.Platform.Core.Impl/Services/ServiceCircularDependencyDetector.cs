namespace DogSab.Platform.Core.Impl.Services;

/// <summary>
/// Tracks which service types are currently being constructed on the calling thread,
/// so that a service whose constructor (directly or transitively) requests itself
/// can be detected and reported instead of causing a stack overflow.
/// </summary>
public class ServiceCircularDependencyDetector
{
    /// <summary>Per-thread stack of service types currently under construction.</summary>
    private readonly ThreadLocal<Stack<Type>> _resolutionStack = new(() => new Stack<Type>());

    /// <summary>
    /// Marks a service type as entering construction on the current thread.
    /// </summary>
    /// <param name="serviceType">The service type about to be constructed.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if <paramref name="serviceType"/> is already present on the current
    /// resolution stack, indicating a circular dependency.
    /// </exception>
    public virtual void Enter(Type serviceType)
    {
        var stack = _resolutionStack.Value!;
        
        if (stack.Contains(serviceType))
        {
            var cyclePath = string.Join(" -> ", stack.Reverse().Select(t => t.Name).Append(serviceType.Name));
            throw new InvalidOperationException($"Circular service dependency detected: {cyclePath}");
        }
        
        stack.Push(serviceType);
    }
    
    /// <summary>
    /// Marks a service type as having finished construction on the current thread,
    /// popping it off the resolution stack.
    /// </summary>
    /// <param name="serviceType">The service type that finished constructing.</param>
    public virtual void Exit(Type serviceType)
    {
        var stack = _resolutionStack.Value!;

        if (stack.Count > 0 && stack.Peek() == serviceType)
        {
            stack.Pop();
        }
    }
}