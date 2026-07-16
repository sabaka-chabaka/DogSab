namespace DogSab.Platform.Core.Abstractions.Components;

/// <summary>Describes where a component currently is in its lifecycle.</summary>
public enum ComponentLifecycleState
{
    /// <summary>The component has been registered but not yet instantiated.</summary>
    NotInitialized,

    /// <summary>The component instance has been created and <c>InitComponent</c> is being called.</summary>
    Initializing,

    /// <summary>The component is fully initialized and ready to use.</summary>
    Initialized,

    /// <summary>The component is being torn down; <c>DisposeComponent</c> is being called.</summary>
    Disposing,

    /// <summary>The component has been fully disposed and must not be used anymore.</summary>
    Disposed
}