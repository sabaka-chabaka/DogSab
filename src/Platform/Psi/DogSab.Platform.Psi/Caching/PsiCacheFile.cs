using System.Collections.Concurrent;
using DogSab.Platform.Core.Abstractions.Logging;
using DogSab.Platform.Core.Abstractions.Messaging;
using DogSab.Platform.Psi.Abstractions.Registry;
using DogSab.Platform.Psi.Abstractions.Tree;
using DogSab.Platform.Psi.Building;
using DogSab.Platform.Vfs.Abstractions.VirtualFile;
using DogSab.Platform.Vfs.Abstractions.Watching;

namespace DogSab.Platform.Psi.Caching;

/// <summary>
/// Caches built <see cref="IPsiFile"/> trees, keyed by virtual path, so
/// repeated requests for the same unchanged file don't re-lex and re-parse
/// it. Invalidated by subscribing to
/// <see cref="VfsTopics.FILE_CHANGED_BACKGROUND"/> — the same background,
/// non-UI-thread topic <c>IndexingFileChangeHandler</c> subscribes to, since
/// PSI invalidation is comparably heavy work that must not run on the UI
/// thread. A changed file's cached tree is simply dropped, not eagerly
/// rebuilt — the next caller to request it pays the rebuild cost, rather
/// than every file edit immediately triggering a reparse whether or not
/// anyone is currently looking at that file.
/// </summary>
public sealed class PsiFileCache : IFileChangeListener
{
    private readonly ConcurrentDictionary<string, IPsiFile> _cacheByPath = new();
    private readonly PsiTreeBuilder _builder;
    private readonly ILanguageRegistry _languageRegistry;
    private readonly ILogger _logger;

    public PsiFileCache(
        PsiTreeBuilder builder,
        ILanguageRegistry languageRegistry,
        IMessageBus messageBus,
        ILoggerFactory loggerFactory)
    {
        _builder = builder;
        _languageRegistry = languageRegistry;
        _logger = loggerFactory.GetLogger(typeof(PsiFileCache));

        var connection = messageBus.Connect();
        connection.Subscribe(VfsTopics.FILE_CHANGED_BACKGROUND, this);
    }

    /// <summary>
    /// Returns the cached PSI tree for a file, building and caching it on
    /// first request. Returns <c>null</c> if no registered language claims
    /// the file's extension — such files simply have no PSI representation.
    /// </summary>
    /// <param name="file">The file to get a PSI tree for.</param>
    /// <returns>The file's PSI tree, or <c>null</c> if its extension isn't claimed by any registered language.</returns>
    public IPsiFile? GetOrBuild(IVirtualFile file)
    {
        if (_cacheByPath.TryGetValue(file.Path, out var cached))
        {
            return cached;
        }

        var extension = System.IO.Path.GetExtension(file.Name).TrimStart('.');
        var parserDefinition = _languageRegistry.FindByFileExtension(extension);

        if (parserDefinition is null)
        {
            return null;
        }

        try
        {
            var psiFile = _builder.Build(file, parserDefinition);
            _cacheByPath[file.Path] = psiFile;
            return psiFile;
        }
        catch (System.Exception ex)
        {
            _logger.Error("Failed to build PSI tree for file '{0}'", ex, file.Path);
            return null;
        }
    }

    /// <inheritdoc />
    public void OnFileChanged(FileChangeEvent args)
    {
        _cacheByPath.TryRemove(args.Path, out _);
    }
}