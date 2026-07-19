namespace DogSab.Platform.Core.Threading.Impl.Diagnostics;

/// <summary>
/// Thrown when code attempts to mutate the platform's shared model (VFS, PSI,
/// ProjectModel, etc.) outside a write action, or otherwise violates the
/// platform's read/write threading contract. Distinct from the plain
/// <see cref="InvalidOperationException"/> thrown by <c>UiThreadGuard</c>, which
/// concerns the UI thread specifically rather than the read/write model lock.
/// </summary>
public sealed class ThreadingViolationException : Exception
{
    /// <summary>The kind of violation that was detected.</summary>
    public ThreadingViolationKind Kind { get; }

    /// <summary>
    /// Creates a new exception describing a threading contract violation.
    /// </summary>
    /// <param name="kind">The kind of violation that was detected.</param>
    /// <param name="message">A message describing the specific violation.</param>
    public ThreadingViolationException(ThreadingViolationKind kind, string message)
        : base(message)
    {
        Kind = kind;
    }
}

/// <summary>Categorizes the kind of threading contract violation detected.</summary>
public enum ThreadingViolationKind
{
    /// <summary>A write operation was attempted without holding the write lock.</summary>
    WriteOutsideWriteAction,

    /// <summary>A write action was attempted while the calling thread already holds a read lock.</summary>
    WriteAttemptedInsideReadAction,

    /// <summary>An operation required to run on the UI thread was called from a different thread.</summary>
    NotOnUiThread
}