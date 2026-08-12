namespace DogSab.Platform.ProjectModel.Abstractions.Roots;

/// <summary>
/// Classifies the purpose of a <see cref="ISourceFolder"/> within a module's
/// content root, so subsystems like Indexing and Psi know how to treat files
/// under it — e.g. test files are indexed but typically excluded from
/// production compilation, and excluded folders are skipped entirely.
/// </summary>
public enum SourceRootType
{
    /// <summary>Production source code.</summary>
    Source,

    /// <summary>Test source code.</summary>
    Test,

    /// <summary>Non-code resource files (e.g. embedded assets, config templates).</summary>
    Resource,

    /// <summary>Excluded from indexing and compilation entirely (e.g. build output, package caches).</summary>
    Excluded,

    /// <summary>Machine-generated source code (e.g. from a source generator), typically indexed but not hand-edited.</summary>
    Generated
}