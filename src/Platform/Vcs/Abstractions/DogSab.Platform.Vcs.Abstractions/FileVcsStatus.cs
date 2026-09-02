namespace DogSab.Platform.Vcs.Abstractions;

/// <summary>
/// The version-control status of a single file, relative to whatever
/// baseline the active <see cref="IVcsProvider"/> considers "clean" (e.g.
/// the current commit for Git). Reported per file by
/// <see cref="IVcsProvider.GetStatus"/> and used by
/// <see cref="IChangeListManager"/> to group changed files, and by a
/// future Project View integration to color file names by their status
/// (the familiar green/blue/red coloring convention most IDEs use for
/// added/modified/conflicted files).
/// </summary>
public enum FileVcsStatus
{
    /// <summary>
    /// The file is tracked by version control and matches the baseline —
    /// no local changes.
    /// </summary>
    Unmodified,

    /// <summary>
    /// The file is tracked and has been locally modified relative to the baseline.
    /// </summary>
    Modified,

    /// <summary>
    /// The file is new and has been staged/marked to be added to version
    /// control, but is not yet committed.
    /// </summary>
    Added,

    /// <summary>
    /// The file was tracked but has been deleted locally, with the
    /// deletion not yet committed.
    /// </summary>
    Deleted,

    /// <summary>
    /// The file exists on disk but is not tracked by version control at
    /// all (e.g. a newly created file the user hasn't added yet).
    /// </summary>
    Untracked,

    /// <summary>
    /// The file has conflicting changes from a merge or rebase operation
    /// that have not yet been resolved.
    /// </summary>
    Conflicted
}