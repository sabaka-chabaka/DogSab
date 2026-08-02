using System.Reflection;
using System.Runtime.Loader;

namespace DogSab.Platform.PluginSystem.Loading;

/// <summary>
/// An isolated <see cref="AssemblyLoadContext"/> for a single plugin,
/// created with <c>isCollectible: true</c> so it can later be unloaded and
/// its memory reclaimed once the platform no longer holds any references
/// into it — see <see cref="Unloading.PluginUnloadCoordinator"/> for the
/// actual unload sequence. Each plugin gets its own context so that two
/// plugins can depend on different, even incompatible, versions of the same
/// third-party library without conflicting.
/// </summary>
/// <remarks>
/// Critical invariant: platform contract assemblies (anything named
/// <c>DogSab.Platform.*</c>) must always resolve to the single copy already
/// loaded in <see cref="AssemblyLoadContext.Default"/>, never to a duplicate
/// copy that might happen to be sitting in the plugin's own directory. The
/// CLR treats a type as distinct per loaded assembly instance — even if two
/// assemblies are byte-for-byte identical, a type loaded from a copy in this
/// context is NOT the same type as the one loaded in the default context. If
/// a plugin's own copy of e.g. <c>DogSab.Platform.Extensibility.Abstractions.dll</c>
/// were loaded here, casting a plugin-implemented <c>ICompletionProvider</c>
/// to the platform's <c>ICompletionProvider</c> would silently fail (via
/// <c>is</c>) or throw <see cref="InvalidCastException"/> (via an explicit
/// cast), because the two identically-named interfaces would be unrelated
/// types from the CLR's perspective. <see cref="Load"/> therefore explicitly
/// refuses to resolve platform assemblies from this context, forcing them to
/// fall back to the default context instead.
/// </remarks>
public sealed class PluginAssemblyLoadContext : AssemblyLoadContext
{
    /// <summary>
    /// The prefix identifying platform contract assemblies that must never be
    /// loaded into a plugin's isolated context — see the type-level remarks.
    /// </summary>
    private const string PlatformAssemblyPrefix = "DogSab.Platform.";

    /// <summary>Resolves the plugin's own dependency DLLs sitting alongside its main assembly.</summary>
    private readonly AssemblyDependencyResolver _dependencyResolver;

    /// <summary>
    /// Creates a new isolated, collectible load context for a plugin.
    /// </summary>
    /// <param name="pluginId">The plugin's identifier, used as this context's name for diagnostics (visible in memory dumps and debugger tooling).</param>
    /// <param name="mainAssemblyPath">The absolute path to the plugin's main assembly file, used to resolve its co-located dependency DLLs.</param>
    public PluginAssemblyLoadContext(string pluginId, string mainAssemblyPath)
        : base(name: pluginId, isCollectible: true)
    {
        _dependencyResolver = new AssemblyDependencyResolver(mainAssemblyPath);
    }

    /// <summary>
    /// Resolves an assembly reference from within this plugin's own directory
    /// before falling back to the default context — except for platform
    /// contract assemblies, which are always forced to fall back to the
    /// default context regardless of whether a copy exists in the plugin's
    /// own directory. See the type-level remarks for why this is critical.
    /// </summary>
    /// <param name="assemblyName">The name of the assembly being resolved.</param>
    /// <returns>The loaded assembly, or <c>null</c> to fall back to default resolution.</returns>
    protected override Assembly? Load(AssemblyName assemblyName)
    {
        if (IsPlatformAssembly(assemblyName))
        {
            // Returning null forces the CLR to fall back to resolving this
            // assembly through the default load context instead, which is
            // guaranteed to already hold the platform's own single copy.
            return null;
        }

        var resolvedPath = _dependencyResolver.ResolveAssemblyToPath(assemblyName);

        return resolvedPath is not null ? LoadFromAssemblyPath(resolvedPath) : null;
    }

    /// <summary>
    /// Resolves a native (unmanaged) library reference from within this
    /// plugin's own directory, for plugins that bundle native dependencies.
    /// </summary>
    /// <param name="unmanagedDllName">The name of the native library being resolved.</param>
    /// <returns>A handle to the loaded native library, or <see cref="IntPtr.Zero"/> to fall back to default resolution.</returns>
    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        var resolvedPath = _dependencyResolver.ResolveUnmanagedDllToPath(unmanagedDllName);

        return resolvedPath is not null ? LoadUnmanagedDllFromPath(resolvedPath) : IntPtr.Zero;
    }

    /// <summary>
    /// Loads the plugin's main assembly into this context. The main assembly
    /// itself is never a platform assembly (it is always the plugin's own
    /// code), so this bypasses the <see cref="Load"/> guard intentionally —
    /// that guard only applies to assemblies referenced *from* the plugin,
    /// not the plugin's own entry assembly.
    /// </summary>
    /// <param name="mainAssemblyPath">The absolute path to the plugin's main assembly file.</param>
    /// <returns>The loaded main assembly.</returns>
    public Assembly LoadMainAssembly(string mainAssemblyPath)
    {
        return LoadFromAssemblyPath(mainAssemblyPath);
    }

    /// <summary>
    /// Checks whether an assembly name identifies a platform contract
    /// assembly that must be resolved from the default context.
    /// </summary>
    /// <param name="assemblyName">The assembly name to check.</param>
    /// <returns><c>true</c> if this is a platform assembly; otherwise <c>false</c>.</returns>
    private static bool IsPlatformAssembly(AssemblyName assemblyName)
    {
        return assemblyName.Name?.StartsWith(PlatformAssemblyPrefix, StringComparison.Ordinal) == true;
    }
}