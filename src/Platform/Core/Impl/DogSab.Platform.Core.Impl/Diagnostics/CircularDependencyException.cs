namespace DogSab.Platform.Core.Impl.Diagnostics;

/// <summary>
/// Thrown when the component dependency resolver detects a cycle
/// among <see cref="Abstractions.Components.ComponentDependsOnAttribute"/> declarations.
/// </summary>
public sealed class CircularDependencyException : Exception
{
    /// <summary>
    /// Creates a new exception describing a detected dependency cycle.
    /// </summary>
    /// <param name="cyclePath">A human-readable path of the types forming the cycle.</param>
    public CircularDependencyException(string cyclePath)
        : base($"Circular component dependency detected: {cyclePath}")
    {
    }
}