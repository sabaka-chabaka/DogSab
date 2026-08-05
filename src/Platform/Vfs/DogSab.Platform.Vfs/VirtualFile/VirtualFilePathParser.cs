using DogSab.Platform.Vfs.Abstractions.FileSystem;

namespace DogSab.Platform.Vfs.VirtualFile;

/// <summary>
/// Splits a full virtual path (e.g. <c>"file:///home/user/project/Program.cs"</c>)
/// into its scheme (<c>"file"</c>) and the remaining path portion
/// (<c>"/home/user/project/Program.cs"</c>). The single place in the platform
/// that understands the virtual path string format, so both
/// <see cref="FileSystem.VirtualFileSystemRouter"/> and individual
/// <see cref="IVirtualFileSystem"/> providers parse
/// paths identically rather than each implementing their own splitting logic.
/// </summary>
public static class VirtualFilePathParser
{
    /// <summary>The separator between a path's scheme and its remainder, matching URI convention.</summary>
    private const string SchemeSeparator = "://";

    /// <summary>
    /// Splits a full virtual path into its scheme and remaining path.
    /// </summary>
    /// <param name="fullPath">The full virtual path to parse, e.g. <c>"file:///home/user/x.cs"</c>.</param>
    /// <returns>The parsed scheme and path portion.</returns>
    /// <exception cref="FormatException">
    /// Thrown if <paramref name="fullPath"/> does not contain the scheme separator <c>"://"</c>.
    /// </exception>
    public static ParsedVirtualPath Parse(string fullPath)
    {
        if (string.IsNullOrWhiteSpace(fullPath))
        {
            throw new FormatException("Virtual path must not be null or empty.");
        }
        
        var separatorIndex = fullPath.IndexOf(SchemeSeparator, StringComparison.Ordinal);

        if (separatorIndex < 0)
        {
            throw new FormatException(
                $"Virtual path '{fullPath}' is missing a scheme separator ('{SchemeSeparator}'). " +
                $"Expected format: 'scheme://path'.");
        }

        var scheme = fullPath[..separatorIndex];
        var path = fullPath[(separatorIndex + SchemeSeparator.Length)..];
        
        if (string.IsNullOrEmpty(scheme))
        {
            throw new FormatException($"Virtual path '{fullPath}' has an empty scheme.");
        }

        // Normalize so the path portion always starts with a single leading
        // slash, regardless of whether the caller wrote "file:///x" (three
        // slashes, absolute Unix-style) or "file://x" (two slashes).
        if (!path.StartsWith('/'))
        {
            path = "/" + path;
        }

        return new ParsedVirtualPath(scheme, path);
    }
    
    /// <summary>
    /// Combines a scheme and path back into a full virtual path string, the
    /// inverse of <see cref="Parse"/>.
    /// </summary>
    /// <param name="scheme">The scheme, without the separator.</param>
    /// <param name="path">The path portion, expected to start with a leading slash.</param>
    /// <returns>The combined full virtual path.</returns>
    public static string Combine(string scheme, string path)
    {
        return $"{scheme}{SchemeSeparator}{path}";
    }
}

/// <summary>The result of parsing a full virtual path into its component parts.</summary>
public readonly struct ParsedVirtualPath
{
    /// <summary>The scheme portion (e.g. <c>"file"</c>), matching a constant from <see cref="Abstractions.FileSystem.VirtualFileSystemScheme"/>.</summary>
    public string Scheme { get; }

    /// <summary>The path portion, normalized to start with a leading slash.</summary>
    public string Path { get; }

    /// <summary>
    /// Creates a new parsed path result.
    /// </summary>
    /// <param name="scheme">The scheme portion.</param>
    /// <param name="path">The path portion.</param>
    public ParsedVirtualPath(string scheme, string path)
    {
        Scheme = scheme;
        Path = path;
    }
}