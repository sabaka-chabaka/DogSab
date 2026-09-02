using DogSab.Platform.Vfs.Abstractions.VirtualFile;

namespace DogSab.Platform.Vcs.Abstractions;

/// <summary>
/// Groups currently modified files into named change lists, the familiar
/// concept from most IDEs' VCS integration for organizing in-progress work
/// into logical groups before committing (e.g. separating an unrelated
/// bugfix from a feature's changes so they can be committed independently).
/// A single "default" change list always exists and receives newly
/// detected changes that haven't been explicitly moved elsewhere by the user.
/// </summary>
public interface IChangeListManager
{
    /// <summary>
    /// The name of the change list newly detected changes are placed into
    /// by default.
    /// </summary>
    string DefaultChangeListName { get; }

    /// <summary>
    /// Every currently defined change list name, including
    /// <see cref="DefaultChangeListName"/>.
    /// </summary>
    IReadOnlyList<string> AllChangeListNames { get; }

    /// <summary>
    /// Creates a new, initially empty change list.
    /// </summary>
    /// <param name="name">
    /// The new change list's name. Must not already be in use.
    /// </param>
    void CreateChangeList(string name);

    /// <summary>
    /// Removes a change list. Any files currently in it are moved back
    /// into <see cref="DefaultChangeListName"/> rather than being
    /// forgotten — a change list groups files for organizational purposes
    /// only, and removing the grouping should never cause the platform to
    /// lose track of an actual pending file change.
    /// </summary>
    /// <param name="name">
    /// The name of the change list to remove. Must not be
    /// <see cref="DefaultChangeListName"/>, which always exists.
    /// </param>
    void RemoveChangeList(string name);

    /// <summary>
    /// Moves a file into a specific change list, removing it from
    /// whichever change list it was previously in.
    /// </summary>
    /// <param name="file">
    /// The file to move.
    /// </param>
    /// <param name="changeListName">
    /// The name of the change list to move it into.
    /// </param>
    void MoveFile(IVirtualFile file, string changeListName);

    /// <summary>
    /// Returns every file currently assigned to a given change list.
    /// </summary>
    /// <param name="changeListName">
    /// The change list to query.
    /// </param>
    /// <returns>
    /// The files currently in this change list.
    /// </returns>
    IReadOnlyList<IVirtualFile> GetFilesInChangeList(string changeListName);
}