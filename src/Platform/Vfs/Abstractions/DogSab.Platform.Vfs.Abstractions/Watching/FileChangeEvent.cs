using DogSab.Platform.Vfs.Abstractions.VirtualFile;

namespace DogSab.Platform.Vfs.Abstractions.Watching;

/// <summary>An immutable record of a single change observed on a virtual file.</summary>
public readonly struct FileChangeEvent
{
    /// <summary>The file or directory that changed.</summary>
    public IVirtualFile File { get; }

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
    /// <param name="file">The file or directory that changed.</param>
    /// <param name="changeType">The kind of change that occurred.</param>
    /// <param name="timestampUtc">The UTC timestamp the change was observed at.</param>
    /// <param name="previousPath">The file's previous path, for <see cref="FileChangeType.Renamed"/> events only.</param>
    public FileChangeEvent(IVirtualFile file, FileChangeType changeType, DateTime timestampUtc, string? previousPath = null)
    {
        File = file;
        ChangeType = changeType;
        TimestampUtc = timestampUtc;
        PreviousPath = previousPath;
    }
}