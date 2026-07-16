namespace DogSab.Platform.Core.Abstractions.Components;

/// <summary>
/// Base marker interface for all platform components.
/// A component is a long-lived entity with an explicit lifecycle,
/// as opposed to IService (lightweight, without explicit init/dispose).
/// </summary>
public interface IComponent
{
}