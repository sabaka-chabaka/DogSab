namespace DogSab.Platform.Editor.Abstractions.Inspections;

/// <summary>A single diagnostic finding reported by an <see cref="IInspection"/> against a specific range of a file.</summary>
public readonly struct Problem
{
    /// <summary>The character offset where the problem's range starts.</summary>
    public int StartOffset { get; }

    /// <summary>The character offset where the problem's range ends.</summary>
    public int EndOffset { get; }

    /// <summary>The human-readable message describing the problem, shown as a tooltip on hover.</summary>
    public string Message { get; }

    /// <summary>How seriously this problem should be treated.</summary>
    public ProblemSeverity Severity { get; }

    /// <summary>
    /// Creates a new problem.
    /// </summary>
    /// <param name="startOffset">The character offset where the problem's range starts.</param>
    /// <param name="endOffset">The character offset where the problem's range ends.</param>
    /// <param name="message">The human-readable message.</param>
    /// <param name="severity">How seriously this problem should be treated.</param>
    public Problem(int startOffset, int endOffset, string message, ProblemSeverity severity)
    {
        StartOffset = startOffset;
        EndOffset = endOffset;
        Message = message;
        Severity = severity;
    }
}