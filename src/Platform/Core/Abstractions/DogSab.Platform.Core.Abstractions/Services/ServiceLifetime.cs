namespace DogSab.Platform.Core.Abstractions.Services;

/// <summary>Defines how a service instance is created and reused by the container.</summary>
public enum ServiceLifetime
{
    /// <summary>A single instance is created and reused for every request within its scope.</summary>
    Singleton,

    /// <summary>A new instance is created for every request.</summary>
    Transient,

    /// <summary>A single instance is reused within a specific logical scope (e.g. one operation).</summary>
    Scoped
}