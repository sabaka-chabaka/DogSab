namespace DogSab.Platform.ProjectModel.Abstractions.Module;

/// <summary>
/// Declares that one module depends on another, e.g. for compiling in the
/// correct order or resolving a combined classpath/reference list. Unlike
/// <c>PluginDependencyDescriptor</c> (Extensibility.Abstractions), this has
/// no version range — module dependencies within a single project are
/// resolved by identity alone (modules of the same project always build
/// together at the same revision), not by independently versioned releases.
/// </summary>
public readonly struct ModuleDependency
{
    /// <summary>The identifier of the module being depended on.</summary>
    public ModuleId DependencyModuleId { get; }

    /// <summary>
    /// Whether this dependency's public types/exports are visible to modules
    /// that in turn depend on the dependent module (a "transitive" or
    /// "exported" dependency), as opposed to being private to the dependent
    /// module's own implementation.
    /// </summary>
    public bool IsExported { get; }

    /// <summary>
    /// Creates a new module dependency declaration.
    /// </summary>
    /// <param name="dependencyModuleId">The identifier of the module being depended on.</param>
    /// <param name="isExported">Whether this dependency is transitively visible to dependents. Defaults to <c>false</c>.</param>
    public ModuleDependency(ModuleId dependencyModuleId, bool isExported = false)
    {
        DependencyModuleId = dependencyModuleId;
        IsExported = isExported;
    }

    /// <inheritdoc />
    public override string ToString() => IsExported ? $"{DependencyModuleId} (exported)" : DependencyModuleId.ToString();
}