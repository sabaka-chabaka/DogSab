namespace DogSab.Platform.Editor.Abstractions.Inspections;

/// <summary>How seriously an inspection-reported problem should be treated, determining its visual presentation (e.g. squiggly underline color) in the editor.</summary>
public enum ProblemSeverity
{
    /// <summary>A non-issue suggestion (e.g. a style hint), shown subtly and not counted as an error/warning.</summary>
    Info,

    /// <summary>A potential issue that doesn't prevent the code from working (e.g. an unused variable).</summary>
    Warning,

    /// <summary>A definite issue (e.g. a syntax error or type mismatch).</summary>
    Error
}