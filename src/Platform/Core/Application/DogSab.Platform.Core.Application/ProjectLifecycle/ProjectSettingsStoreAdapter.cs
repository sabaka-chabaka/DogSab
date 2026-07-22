using DogSab.Platform.Core.Settings.Impl.Store;

namespace DogSab.Platform.Core.Application.ProjectLifecycle;

/// <summary>
/// Adapts a project-rooted <see cref="SettingsStoreImpl"/> instance to
/// <see cref="ISettingsStoreForProject"/>, so it is type-distinguishable from
/// the application-level settings store within <see cref="ProjectSession"/>.
/// </summary>
internal sealed class ProjectSettingsStoreAdapter : ISettingsStoreForProject
{
    private readonly SettingsStoreImpl _inner;

    public ProjectSettingsStoreAdapter(SettingsStoreImpl inner)
    {
        _inner = inner;
    }

    public T Load<T>() where T : class, new() => _inner.Load<T>();
    public void Save<T>(T instance) where T : class => _inner.Save(instance);
}