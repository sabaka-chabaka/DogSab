using System.Collections.Concurrent;
using DogSab.Platform.Vcs.Abstractions;
using DogSab.Platform.Vfs.Abstractions.VirtualFile;

namespace DogSab.Platform.Vcs;

/// <summary>
/// Default implementation of <see cref="IChangeListManager"/>.
/// </summary>
public sealed class ChangeListManagerImpl : IChangeListManager
{
    /// <inheritdoc />
    public string DefaultChangeListName => "Default";

    /// <summary>
    /// Files currently assigned to each change list, keyed by change list
    /// name. The default change list's entry always exists; other entries
    /// exist only for change lists the user has explicitly created.
    /// </summary>
    private readonly ConcurrentDictionary<string, HashSet<IVirtualFile>> _filesByChangeList = new();

    /// <summary>
    /// Creates a new change list manager, with the default change list
    /// already present.
    /// </summary>
    public ChangeListManagerImpl()
    {
        _filesByChangeList[DefaultChangeListName] = new HashSet<IVirtualFile>();
    }

    /// <inheritdoc />
    public IReadOnlyList<string> AllChangeListNames => _filesByChangeList.Keys.ToList();

    /// <inheritdoc />
    public void CreateChangeList(string name)
    {
        if (!_filesByChangeList.TryAdd(name, new HashSet<IVirtualFile>()))
        {
            throw new InvalidOperationException($"A change list named '{name}' already exists.");
        }
    }

    /// <inheritdoc />
    public void RemoveChangeList(string name)
    {
        if (name == DefaultChangeListName)
        {
            throw new InvalidOperationException($"The default change list ('{DefaultChangeListName}') cannot be removed.");
        }

        if (!_filesByChangeList.TryRemove(name, out var filesInRemovedList))
        {
            return;
        }

        // Files in the removed list are not simply discarded — they still
        // represent real pending changes on disk, so they are folded back
        // into the default change list rather than being forgotten.
        var defaultList = _filesByChangeList[DefaultChangeListName];
        lock (defaultList)
        {
            foreach (var file in filesInRemovedList)
            {
                defaultList.Add(file);
            }
        }
    }

    /// <inheritdoc />
    public void MoveFile(IVirtualFile file, string changeListName)
    {
        if (!_filesByChangeList.TryGetValue(changeListName, out var targetList))
        {
            throw new InvalidOperationException($"No change list named '{changeListName}' exists.");
        }

        foreach (var list in _filesByChangeList.Values)
        {
            lock (list)
            {
                list.Remove(file);
            }
        }

        lock (targetList)
        {
            targetList.Add(file);
        }
    }

    /// <inheritdoc />
    public IReadOnlyList<IVirtualFile> GetFilesInChangeList(string changeListName)
    {
        if (!_filesByChangeList.TryGetValue(changeListName, out var list))
        {
            return Array.Empty<IVirtualFile>();
        }

        lock (list)
        {
            return list.ToList();
        }
    }
}