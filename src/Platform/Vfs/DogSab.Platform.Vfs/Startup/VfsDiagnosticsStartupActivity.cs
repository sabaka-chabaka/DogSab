using DogSab.Platform.Core.Abstractions.Lifecycle;
using DogSab.Platform.Core.Abstractions.Logging;
using DogSab.Platform.Vfs.FileSystem;

namespace DogSab.Platform.Vfs.Startup;

/// <summary>
/// Platform startup activity that logs which file system providers are
/// registered, mirroring the diagnostics pattern already used for Messaging
/// and Extensibility. Useful to confirm at a glance that both
/// <see cref="LocalFileSystem"/> and <see cref="InMemoryFileSystem"/> were
/// registered successfully during bootstrap.
/// </summary>
public sealed class VfsDiagnosticsStartupActivity : IStartupActivity
{
    private readonly VirtualFileSystemRegistry _registry;
    private readonly ILoggerFactory _loggerFactory;

    public VfsDiagnosticsStartupActivity(VirtualFileSystemRegistry registry, ILoggerFactory loggerFactory)
    {
        _registry = registry;
        _loggerFactory = loggerFactory;
    }

    public int Order => 1500;

    public Task RunActivityAsync(CancellationToken cancellationToken)
    {
        var logger = _loggerFactory.GetLogger(typeof(VfsDiagnosticsStartupActivity));

        foreach (var scheme in new[] { "file", "memory", "archive" })
        {
            logger.Debug(
                "VFS scheme '{0}': {1}",
                scheme,
                _registry.IsRegistered(scheme) ? "registered" : "not registered");
        }

        return Task.CompletedTask;
    }
}