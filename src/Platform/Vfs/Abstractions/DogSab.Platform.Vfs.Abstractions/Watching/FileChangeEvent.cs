using DogSab.Platform.Vfs.Abstractions.VirtualFile;

namespace DogSab.Platform.Vfs.Abstractions.Watching;

/// <summary>
/// An immutable record of a single change observed on a virtual file. For
/// <see cref="FileChangeType.Deleted"/> events, <see cref="File"/> is
/// <c>null</c> — a deleted file has no meaningful <see cref="IVirtualFile"/>
/// to offer (it no longer exists to read from or enumerate children of), so
/// subscribers reacting to deletion should use <see cref="Path"/> alone.
/// </summary>
public readonly struct FileChangeEvent
{
    /// <summary>
    /// The changed file or directory, or <c>null</c> if <see cref="ChangeType"/>
    /// is <see cref="FileChangeType.Deleted"/>.
    /// </summary>
    public IVirtualFile? File { get; }

    /// <summary>
    /// The full virtual path of the affected file, always populated
    /// regardless of <see cref="ChangeType"/> — the one piece of information
    /// guaranteed available even when <see cref="File"/> is <c>null</c>.
    /// </summary>
    public string Path { get; }

    /// <summary>The kind of change that occurred.</summary>
    public FileChangeType ChangeType { get; }

    /// <summary>
    /// For <see cref="FileChangeType.Renamed"/> events, the file's previous
    /// full path; <c>null</c> for all other change types.
    /// </summary>
    public string? PreviousPath { get; }

    /// <summary>The UTC timestamp the change was observed at.</summary>
    public DateTime TimestampUtc { get; }

    /// <summary>
    /// Creates a new file change event record.
    /// </summary>
    /// <param name="path">The full virtual path of the affected file.</param>
    /// <param name="changeType">The kind of change that occurred.</param>
    /// <param name="timestampUtc">The UTC timestamp the change was observed at.</param>
    /// <param name="file">The changed file, or <c>null</c> for a deletion.</param>
    /// <param name="previousPath">The file's previous path, for <see cref="FileChangeType.Renamed"/> events only.</param>
    public FileChangeEvent(string path, FileChangeType changeType, DateTime timestampUtc, IVirtualFile? file = null, string? previousPath = null)
    {
        Path = path;
        ChangeType = changeType;
        TimestampUtc = timestampUtc;
        File = file;
        PreviousPath = previousPath;
    }
}