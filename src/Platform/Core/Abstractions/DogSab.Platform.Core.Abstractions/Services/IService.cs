namespace DogSab.Platform.Core.Abstractions.Services;

/// <summary>
/// Marker for a service — a lightweight entity without a mandatory explicit lifecycle.
/// Unlike IComponent, services are typically resolved lazily on demand.
/// </summary>
public interface IService
{
}