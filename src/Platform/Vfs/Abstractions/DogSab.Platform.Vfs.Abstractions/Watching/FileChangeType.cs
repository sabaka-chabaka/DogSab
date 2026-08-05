namespace DogSab.Platform.Vfs.Abstractions.Watching;

/// <summary>Identifies the kind of change that happened to a virtual file.</summary>
public enum FileChangeType
{
    /// <summary>A new file or directory was created.</summary>
    Created,

    /// <summary>An existing file's content was modified.</summary>
    Changed,

    /// <summary>A file or directory was deleted.</summary>
    Deleted,

    /// <summary>A file or directory was renamed or moved.</summary>
    Renamed
}