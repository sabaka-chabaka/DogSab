namespace DogSab.Platform.Core.Logging.Impl.Providers;

/// Owns the actual file handle for the platform's log output and performs
/// size-based rotation: when the current file would exceed the configured
/// maximum size, it is renamed with a numeric suffix and a fresh file is
/// started, with the oldest rotated files deleted once the retention limit
/// is exceeded. Shared by every <see cref="RollingFileLogger"/> instance
/// created by the same <see cref="RollingFileLoggerProvider"/>, since all
/// categories write to the same physical file.
/// </summary>
public sealed class RollingFileWriter : IDisposable
{
    /// <summary>The path of the currently active (non-rotated) log file.</summary>
    private readonly string _currentFilePath;

    /// <summary>The maximum size, in bytes, the current file may reach before rotation.</summary>
    private readonly long _maxFileSizeBytes;

    /// <summary>How many rotated files are retained before the oldest is deleted.</summary>
    private readonly int _retainedFileCount;

    /// <summary>Serializes all writes and rotation checks, since multiple loggers/threads write concurrently.</summary>
    private readonly object _writeLock = new();

    /// <summary>
    /// Creates a new rolling file writer.
    /// </summary>
    /// <param name="currentFilePath">The path of the active log file.</param>
    /// <param name="maxFileSizeBytes">The maximum size, in bytes, before rotation occurs.</param>
    /// <param name="retainedFileCount">How many rotated files to retain.</param>
    public RollingFileWriter(string currentFilePath, long maxFileSizeBytes, int retainedFileCount)
    {
        _currentFilePath = currentFilePath;
        _maxFileSizeBytes = maxFileSizeBytes;
        _retainedFileCount = retainedFileCount;
    }

    /// <summary>
    /// Appends a single formatted line to the current log file, rotating first
    /// if the file has reached its configured maximum size.
    /// </summary>
    /// <param name="line">The already-formatted line to append (without a trailing newline).</param>
    public void AppendLine(string line)
    {
        lock (_writeLock)
        {
            RotateIfNeeded();

            using var stream = new FileStream(_currentFilePath, FileMode.Append, FileAccess.Write, FileShare.Read);
            using var writer = new StreamWriter(stream);
            writer.WriteLine(line);
        }
    }

    /// <summary>
    /// Rotates the current log file if it exists and has reached or exceeded the
    /// configured maximum size. Renames it with a <c>.1</c> suffix, shifting any
    /// previously rotated files up by one index, and deletes the oldest file once
    /// <see cref="_retainedFileCount"/> is exceeded. Must be called while holding <see cref="_writeLock"/>.
    /// </summary>
    private void RotateIfNeeded()
    {
        var fileInfo = new FileInfo(_currentFilePath);

        if (!fileInfo.Exists || fileInfo.Length < _maxFileSizeBytes)
        {
            return;
        }

        var oldestIndexPath = $"{_currentFilePath}.{_retainedFileCount}";
        if (File.Exists(oldestIndexPath))
        {
            File.Delete(oldestIndexPath);
        }

        for (var index = _retainedFileCount - 1; index >= 1; index--)
        {
            var source = $"{_currentFilePath}.{index}";
            var destination = $"{_currentFilePath}.{index + 1}";

            if (File.Exists(source))
            {
                File.Move(source, destination);
            }
        }

        File.Move(_currentFilePath, $"{_currentFilePath}.1");
    }

    /// <summary>
    /// No unmanaged resources are held between calls to <see cref="AppendLine"/>
    /// (the file stream is opened and closed per write), so disposal is a no-op.
    /// Present to satisfy ownership expectations from <see cref="RollingFileLoggerProvider"/>.
    /// </summary>
    public void Dispose()
    {
    }
}