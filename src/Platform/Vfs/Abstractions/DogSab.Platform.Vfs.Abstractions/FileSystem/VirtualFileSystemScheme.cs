namespace DogSab.Platform.Vfs.Abstractions.FileSystem;

/// <summary>
/// String constants for the scheme prefixes used to disambiguate which
/// backing <see cref="IVirtualFileSystem"/> a virtual path belongs to, e.g.
/// <c>"file:///home/user/project/Program.cs"</c> vs
/// <c>"archive:///home/user/lib.nupkg!/lib/net9.0/Foo.dll"</c>. Declared as
/// string constants rather than an enum so that new file system providers
/// (including ones contributed by plugins) can introduce their own scheme
/// without requiring a change to this platform assembly.
/// </summary>
public static class VirtualFileSystemScheme
{
    /// <summary>Scheme for files backed by the local disk.</summary>
    public const string Local = "file";

    /// <summary>Scheme for files that exist only in memory (e.g. generated/scratch content), never persisted to disk.</summary>
    public const string InMemory = "memory";

    /// <summary>Scheme for files viewed inside an archive (e.g. a <c>.zip</c> or <c>.nupkg</c>) without extracting it.</summary>
    public const string Archive = "archive";
}