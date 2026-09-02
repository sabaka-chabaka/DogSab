namespace DogSab.Platform.Vcs.Abstractions;

/// <summary>
/// Describes a single historical commit, as reported by an
/// <see cref="IVcsProvider"/> when browsing a file or repository's history
/// (e.g. for a future "Show History" or "Annotate/Blame" feature).
/// Deliberately narrow — just the metadata every VCS has a direct
/// equivalent for — rather than exposing VCS-specific concepts like Git's
/// parent commits or branch refs, which not every version control system
/// shares.
/// </summary>
public interface ICommitInfo
{
    /// <summary>
    /// The commit's unique identifier, in whatever form the underlying VCS
    /// uses (e.g. a Git SHA hash).
    /// </summary>
    string CommitId { get; }

    /// <summary>
    /// The name of the person who authored the commit.
    /// </summary>
    string AuthorName { get; }

    /// <summary>
    /// The commit's message, as written by its author.
    /// </summary>
    string Message { get; }

    /// <summary>
    /// The date and time the commit was made.
    /// </summary>
    DateTimeOffset Timestamp { get; }
}