namespace DogSab.Platform.Core.Abstractions.Disposables;

/// <summary>
/// Marks a class for automatic registration in the disposable tree
/// when create through the platform's DI container.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class DisposableAttribute : Attribute
{
    
}