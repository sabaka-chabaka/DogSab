using System.Collections.Concurrent;
using DogSab.Platform.Core.Abstractions.Logging;
using DogSab.Platform.Core.Abstractions.Messaging;
using DogSab.Platform.Vfs.Abstractions.Documents;
using DogSab.Platform.Vfs.Abstractions.Watching;

namespace DogSab.Platform.Vfs.Documents;

/// <summary>
/// Reconciles external file changes with open documents by consulting a
/// registered <see cref="IDocumentSyncListener"/> for each affected file.
/// Handles all three possible outcomes: applies <see cref="DocumentSyncResolution.ReloadFromDisk"/>
/// and <see cref="DocumentSyncResolution.KeepInMemoryVersion"/> immediately,
/// and for <see cref="DocumentSyncResolution.Deferred"/> — typically because
/// the listener needs to prompt the user asynchronously — holds the pending
/// change until <see cref="ResolveDeferred"/> is called explicitly, rather
/// than blocking the thread that detected the external change.
/// </summary>
public sealed class DocumentSyncCoordinator
{
    private readonly IDocumentSyncListener _syncListener;
    private readonly ILogger _logger;

    /// <summary>Changes awaiting an explicit resolution, keyed by the affected file's virtual path.</summary>
    private readonly ConcurrentDictionary<string, FileChangeEvent> _pendingResolutions = new();

    /// <summary>
    /// Creates a new document sync coordinator.
    /// </summary>
    /// <param name="syncListener">The listener consulted to resolve conflicts between external changes and open documents.</param>
    /// <param name="messageBus">The message bus to subscribe to file change notifications on.</param>
    /// <param name="loggerFactory">Factory used to obtain a logger scoped to this coordinator.</param>
    public DocumentSyncCoordinator(IDocumentSyncListener syncListener, IMessageBus messageBus, ILoggerFactory loggerFactory)
    {
        _syncListener = syncListener;
        _logger = loggerFactory.GetLogger(typeof(DocumentSyncCoordinator));

        var connection = messageBus.Connect();
        connection.Subscribe(VfsTopics.FILE_CHANGED_UI, new FileChangeHandler(this));
    }

    /// <summary>
    /// Handles a single file change notification by consulting the sync
    /// listener and acting on its resolution.
    /// </summary>
    /// <param name="changeEvent">The change to reconcile.</param>
    internal void HandleChange(FileChangeEvent changeEvent)
    {
        if (changeEvent.File is null)
        {
            // A deletion with no open document to reconcile against is not this coordinator's concern.
            return;
        }

        var resolution = _syncListener.OnExternalFileChanged(changeEvent.File);

        switch (resolution)
        {
            case DocumentSyncResolution.ReloadFromDisk:
                _logger.Debug("Reloading document for '{0}' from disk after external change.", changeEvent.Path);
                break;

            case DocumentSyncResolution.KeepInMemoryVersion:
                _logger.Debug("Keeping in-memory document for '{0}', ignoring external change.", changeEvent.Path);
                break;

            case DocumentSyncResolution.Deferred:
                _pendingResolutions[changeEvent.Path] = changeEvent;
                _logger.Debug("Deferred resolution for '{0}'; awaiting explicit ResolveDeferred call.", changeEvent.Path);
                break;
        }
    }

    /// <summary>
    /// Explicitly resolves a previously deferred change — called once the
    /// party that returned <see cref="DocumentSyncResolution.Deferred"/> has
    /// made its decision (e.g. after the user answers a prompt).
    /// </summary>
    /// <param name="path">The virtual path of the file whose deferred change should now be resolved.</param>
    /// <param name="finalResolution">
    /// The final resolution to apply. Must not be <see cref="DocumentSyncResolution.Deferred"/> again.
    /// </param>
    /// <returns><c>true</c> if a pending deferred change was found and resolved; otherwise <c>false</c>.</returns>
    public bool ResolveDeferred(string path, DocumentSyncResolution finalResolution)
    {
        if (finalResolution == DocumentSyncResolution.Deferred)
        {
            throw new ArgumentException("Cannot resolve a deferred change with 'Deferred' again.", nameof(finalResolution));
        }

        if (!_pendingResolutions.TryRemove(path, out _))
        {
            return false;
        }

        _logger.Debug("Resolved deferred change for '{0}' as {1}.", path, finalResolution);
        return true;
    }

    /// <summary>Bridges message bus delivery to <see cref="HandleChange"/>, since <see cref="IMessageBusConnection.Subscribe{T}"/> requires a listener instance.</summary>
    private sealed class FileChangeHandler : IFileChangeListener
    {
        private readonly DocumentSyncCoordinator _owner;

        public FileChangeHandler(DocumentSyncCoordinator owner) => _owner = owner;

        public void OnFileChanged(FileChangeEvent args) => _owner.HandleChange(args);
    }
}