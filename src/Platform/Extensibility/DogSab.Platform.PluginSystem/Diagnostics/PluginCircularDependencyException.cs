namespace DogSab.Platform.PluginSystem.Diagnostics;

/// <summary>
/// Thrown when <see cref="DependencyResolution.PluginDependencyGraphResolver"/>
/// detects a cycle among required plugin dependencies.
/// </summary>
public sealed class PluginCircularDependencyException : Exception
{
    /// <summary>
    /// Creates a new exception describing a detected plugin dependency cycle.
    /// </summary>
    /// <param name="cyclePath">A human-readable path of the plugin IDs forming the cycle.</param>
    public PluginCircularDependencyException(string cyclePath)
        : base($"Circular plugin dependency detected: {cyclePath}")
    {
    }
}