namespace DogSab.Platform.RunConfigurations.Abstractions;

/// <summary>
/// The current lifecycle state of a launched process, tracked by an
/// <see cref="IRunProcessHandle"/>.
/// </summary>
public enum RunState
{
    /// <summary>
    /// The process has been configured but not yet launched.
    /// </summary>
    NotStarted,

    /// <summary>
    /// The process is currently running.
    /// </summary>
    Running,

    /// <summary>
    /// The process exited normally (regardless of its exit code — a
    /// non-zero exit code is still <see cref="Stopped"/>, not
    /// <see cref="Failed"/>, since a program returning a non-zero code is
    /// expected, ordinary behavior for many programs, not a platform-level
    /// launch failure).
    /// </summary>
    Stopped,

    /// <summary>
    /// The platform itself could not launch the process at all (e.g. the
    /// executable path did not exist, or the OS denied permission to
    /// start it) — distinct from <see cref="Stopped"/>, which represents a
    /// process that did start and later exited.
    /// </summary>
    Failed
}