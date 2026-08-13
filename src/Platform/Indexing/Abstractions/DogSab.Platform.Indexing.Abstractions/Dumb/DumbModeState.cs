using DogSab.Platform.Indexing.Abstractions.Index;

namespace DogSab.Platform.Indexing.Abstractions.Dumb;

/// <summary>
/// Indicates whether the platform's indexes can currently be trusted as
/// complete. Named after the equivalent concept in IntelliJ Platform: while
/// indexing is in progress ("dumb mode"), index-backed features either wait
/// or warn the user that results may be incomplete, since files not yet
/// processed by <see cref="IIndexExtension{TKey,TValue}"/> simply have
/// no entries yet — not because they genuinely lack the queried fact, but
/// because they haven't been looked at.
/// </summary>
public enum DumbModeState
{
    /// <summary>All known files have been indexed; index queries reflect the true, complete state.</summary>
    Smart,

    /// <summary>Indexing is currently in progress; index queries may return incomplete results.</summary>
    Dumb
}