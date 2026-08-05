namespace DogSab.Platform.Vfs.Abstractions.VirtualFile;

/// <summary>Distinguishes whether a <see cref="IVirtualFile"/> represents a file or a directory.</summary>
public enum VirtualFileType
{
    /// <summary>A regular file with readable content.</summary>
    File,

    /// <summary>A directory that may contain child files and directories.</summary>
    Directory
}