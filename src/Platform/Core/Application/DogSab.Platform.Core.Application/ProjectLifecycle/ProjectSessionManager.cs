using System.Collections.Concurrent;
using DogSab.Platform.Core.Abstractions.Logging;
using DogSab.Platform.Core.Impl.Components;
using DogSab.Platform.Core.Impl.Services;
using DogSab.Platform.Core.Settings.Impl.Paths;
using DogSab.Platform.Core.Settings.Impl.Store;

namespace DogSab.Platform.Core.Application.ProjectLifecycle;

/// <summary>
/// Opens and closes <see cref="ProjectSession"/> instances, supporting multiple
/// simultaneously open projects (a multi-project workspace). Each session is
/// fully independent: its own component manager, its own service container
/// (delegating to the shared application root container for anything not
/// registered at project scope), and its own settings store.
/// </summary>
public class ProjectSessionManager
{
    /// <summary>Currently open sessions, keyed by their generated project ID.</summary>
    private readonly ConcurrentDictionary<Guid, ProjectSession> _openSessions = new();

    /// <summary>The application's root service container, used as the parent for every project-scoped container.</summary>
    private readonly ServiceContainerImpl _applicationRootContainer;

    /// <summary>Logger factory passed through to project-scoped subsystems that need one.</summary>
    private readonly ILoggerFactory _loggerFactory;

    /// <summary>
    /// Creates a new project session manager.
    /// </summary>
    /// <param name="applicationRootContainer">The application's root service container, used as the parent for project-scoped containers.</param>
    /// <param name="loggerFactory">Factory used to obtain loggers for project-scoped subsystems.</param>
    public ProjectSessionManager(ServiceContainerImpl applicationRootContainer, ILoggerFactory loggerFactory)
    {
        _applicationRootContainer = applicationRootContainer;
        _loggerFactory = loggerFactory;
    }

    /// <summary>Every currently open project session, in no particular order.</summary>
    public IReadOnlyCollection<ProjectSession> OpenSessions => (IReadOnlyCollection<ProjectSession>)_openSessions.Values;

    /// <summary>
    /// Opens a new project session rooted at the given directory.
    /// </summary>
    /// <param name="projectRootDirectory">The absolute path to the project's root directory. Must exist.</param>
    /// <returns>The newly opened session.</returns>
    /// <exception cref="ProjectOpenException">
    /// Thrown if <paramref name="projectRootDirectory"/> does not exist, or if
    /// project component initialization fails.
    /// </exception>
    public virtual ProjectSession OpenProject(string projectRootDirectory)
    {
        if (!Directory.Exists(projectRootDirectory))
        {
            throw new ProjectOpenException(
                projectRootDirectory,
                $"Project root directory '{projectRootDirectory}' does not exist.");
        }

        var logger = _loggerFactory.GetLogger(typeof(ProjectSessionManager));

        try
        {
            var projectId = Guid.NewGuid();

            var componentManager = new ProjectComponentManager(new ComponentDependencyResolver());

            var projectRegistry = new ServiceRegistry();
            var activator = new ServiceActivator(new ServiceCircularDependencyDetector());
            var serviceContainer = new ServiceContainerImpl(
                Abstractions.Services.ServiceScope.Project,
                projectRegistry,
                activator,
                parent: _applicationRootContainer);

            var pathResolver = new SettingsPathResolver(projectRootDirectory);
            var settingsStoreImpl = new SettingsStoreImpl(pathResolver, _loggerFactory);
            var settingsStore = new ProjectSettingsStoreAdapter(settingsStoreImpl);

            var session = new ProjectSession(
                projectId,
                projectRootDirectory,
                componentManager,
                serviceContainer,
                settingsStore);

            _openSessions[projectId] = session;

            componentManager.NotifyProjectOpened();

            logger.Info("Project opened: {0} (id {1})", projectRootDirectory, projectId);

            return session;
        }
        catch (Exception ex) when (ex is not ProjectOpenException)
        {
            logger.Error("Failed to open project at '{0}'", ex, projectRootDirectory);
            throw new ProjectOpenException(projectRootDirectory, $"Failed to open project: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Closes an open project session: notifies its components, disposes them,
    /// and removes it from <see cref="OpenSessions"/>.
    /// </summary>
    /// <param name="projectId">The ID of the session to close, as returned by <see cref="OpenProject"/>.</param>
    /// <returns><c>true</c> if a session with this ID was found and closed; otherwise <c>false</c>.</returns>
    public virtual bool CloseProject(Guid projectId)
    {
        if (!_openSessions.TryRemove(projectId, out var session))
        {
            return false;
        }

        var logger = _loggerFactory.GetLogger(typeof(ProjectSessionManager));

        try
        {
            session.Dispose();
            logger.Info("Project closed: {0} (id {1})", session.ProjectRootDirectory, projectId);
        }
        catch (Exception ex)
        {
            logger.Error("Error while closing project '{0}' (id {1})", ex, session.ProjectRootDirectory, projectId);
        }

        return true;
    }

    /// <summary>
    /// Closes every currently open project session. Used during application
    /// shutdown by <see cref="Shutdown.ApplicationShutdownCoordinator"/>.
    /// </summary>
    public virtual void CloseAllProjects()
    {
        foreach (var projectId in new List<Guid>(_openSessions.Keys))
        {
            CloseProject(projectId);
        }
    }
}