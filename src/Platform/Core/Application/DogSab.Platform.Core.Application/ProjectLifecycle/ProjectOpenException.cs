namespace DogSab.Platform.Core.Application.ProjectLifecycle;

/// <summary>
/// Thrown when a project fails to open — either because its component
/// initialization failed, its root directory is invalid, or it is already open.
/// Wraps the original cause, if any, for diagnostics.
/// </summary>
public sealed class ProjectOpenException : Exception
{
    /// <summary>The root directory of the project that failed to open.</summary>
    public string ProjectRootDirectory { get; }

    /// <summary>
    /// Creates a new exception describing a failed project open attempt.
    /// </summary>
    /// <param name="projectRootDirectory">The root directory of the project that failed to open.</param>
    /// <param name="message">A message describing why the open failed.</param>
    /// <param name="inner">The underlying exception, if any.</param>
    public ProjectOpenException(string projectRootDirectory, string message, Exception? inner = null)
        : base(message, inner)
    {
        ProjectRootDirectory = projectRootDirectory;
    }
}