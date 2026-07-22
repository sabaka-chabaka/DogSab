using DogSab.Platform.Core.Abstractions.Components;
using DogSab.Platform.Core.Abstractions.Services;
using DogSab.Platform.Core.Impl.Components;
using DogSab.Platform.Core.Impl.Services;

namespace DogSab.Platform.Core.Application.ProjectLifecycle;

/// <summary>
/// Holds together every subsystem instance scoped to a single open project:
/// its own <see cref="ProjectComponentManager"/>, a project-scoped
/// <see cref="ServiceContainerImpl"/> whose <see cref="ServiceContainerImpl.Parent"/>
/// is the application's root container, and a project-scoped settings store
/// rooted at the project's own directory. Created and owned exclusively by
/// <see cref="ProjectSessionManager"/>; other code should not construct this directly.
/// </summary>
public sealed class ProjectSession : IDisposable
{
    /// <summary>A stable identifier for this session, used as the key in <see cref="ProjectSessionManager"/>.</summary>
    public Guid ProjectId { get; }

    /// <summary>The absolute root directory of the project this session represents.</summary>
    public string ProjectRootDirectory { get; }

    /// <summary>Manages the lifecycle of components scoped to this project.</summary>
    public IComponentManager ComponentManager { get; }

    /// <summary>The project-scoped dependency injection container, falling back to the application root container.</summary>
    public IServiceContainer ServiceContainer { get; }

    /// <summary>Persists and loads settings scoped to this project (<see cref="Abstractions.Settings.SettingsScope.Project"/> and <see cref="Abstractions.Settings.SettingsScope.Workspace"/>).</summary>
    public ISettingsStoreForProject SettingsStore { get; }

    /// <summary>The underlying project component manager, exposed for internal lifecycle calls not on the public <see cref="IComponentManager"/> contract.</summary>
    internal ProjectComponentManager ComponentManagerImpl { get; }

    /// <summary>Whether this session has already been disposed.</summary>
    private bool _isDisposed;

    /// <summary>
    /// Creates a new project session. Intended to be called only by <see cref="ProjectSessionManager.OpenProject"/>.
    /// </summary>
    /// <param name="projectId">A stable identifier for this session.</param>
    /// <param name="projectRootDirectory">The absolute root directory of the project.</param>
    /// <param name="componentManager">The project-scoped component manager.</param>
    /// <param name="serviceContainer">The project-scoped service container.</param>
    /// <param name="settingsStore">The project-scoped settings store.</param>
    internal ProjectSession(
        Guid projectId,
        string projectRootDirectory,
        ProjectComponentManager componentManager,
        ServiceContainerImpl serviceContainer,
        ISettingsStoreForProject settingsStore)
    {
        ProjectId = projectId;
        ProjectRootDirectory = projectRootDirectory;
        ComponentManagerImpl = componentManager;
        ComponentManager = componentManager;
        ServiceContainer = serviceContainer;
        SettingsStore = settingsStore;
    }

    /// <summary>
    /// Tears down this session: notifies all project components that the
    /// project is closing, disposes them, and clears the project-scoped
    /// service cache. Safe to call multiple times; subsequent calls are no-ops.
    /// </summary>
    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;

        ComponentManagerImpl.DisposeAll();

        if (ServiceContainer is ServiceContainerImpl impl)
        {
            impl.ClearCache();
        }
    }
}