namespace DogSab.Platform.Core.Abstractions.Application;

/// <summary>Identifies a stage in the application's or a project's lifecycle.</summary>
public enum ApplicationLifecycleEvent
{
    /// <summary>The application process has begun starting up.</summary>
    Starting,

    /// <summary>The application has finished starting up and is ready for use.</summary>
    Started,

    /// <summary>A project has begun opening.</summary>
    ProjectOpening,

    /// <summary>A project has finished opening and all its components are initialized.</summary>
    ProjectOpened,

    /// <summary>A project has begun closing.</summary>
    ProjectClosing,

    /// <summary>A project has finished closing and its components have been disposed.</summary>
    ProjectClosed,

    /// <summary>The application process is shutting down.</summary>
    Exiting
}