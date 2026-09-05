using System.Linq;
using System.Reflection;
using DogSab.Platform.Core.Abstractions.Logging;
using DogSab.Platform.Core.Abstractions.Services;
using DogSab.Platform.Extensibility.Abstractions.ExtensionPoints;

namespace DogSab.Platform.Extensibility.Reflection;

/// <summary>
/// Discovers and registers the platform's own internal <c>[Extension]</c>-attributed
/// classes — as opposed to externally loaded plugins, handled by
/// <c>DogSab.Platform.PluginSystem.Loading.PluginLoaderImpl</c> from manifest
/// declarations. Bridges <see cref="ExtensionAttributeScanner"/>'s discovery with
/// actual instantiation (via constructor injection from a service container) and
/// registration into <see cref="IExtensionPointRegistry"/>.
///
/// Nothing calls this yet as of when this class was added — see
/// <see cref="ExtensionAttributeScanner"/>'s own remarks. Wiring it into the
/// platform's actual startup sequence (and deciding exactly when, relative to
/// Avalonia's lifecycle and each extension point's declaration, it should run)
/// is left to whoever integrates it, since that decision depends on details
/// (which assemblies are loaded yet, which extension points have been
/// declared yet) that belong to the host application, not to this generic
/// loader.
///
/// Construction failures — an <c>[Extension]</c>-attributed class whose
/// constructor needs a service that isn't registered, or whose target
/// extension point hasn't been declared yet — are collected into the
/// returned <see cref="ExtensionLoadResult"/> rather than thrown, since large
/// parts of the platform are still being wired into the DI container
/// incrementally. One broken or not-yet-wireable extension should never
/// prevent the rest of the platform, or the application window itself, from
/// starting.
/// </summary>
public sealed class InternalExtensionLoader
{
    private readonly IServiceContainer _container;
    private readonly IExtensionPointRegistry _registry;
    private readonly ExtensionAttributeScanner _scanner = new();

    /// <summary>
    /// Creates a new loader.
    /// </summary>
    /// <param name="container">The container used to resolve each extension's constructor dependencies.</param>
    /// <param name="registry">The registry each successfully constructed extension is registered into.</param>
    public InternalExtensionLoader(IServiceContainer container, IExtensionPointRegistry registry)
    {
        _container = container;
        _registry = registry;
    }

    /// <summary>
    /// Scans every given assembly for <c>[Extension]</c>-attributed classes and
    /// attempts to instantiate and register each one found.
    /// </summary>
    /// <param name="assemblies">The already-loaded assemblies to scan.</param>
    /// <returns>A summary of what was registered and what was skipped, and why.</returns>
    public ExtensionLoadResult ScanAndRegister(IEnumerable<Assembly> assemblies)
    {
        var registered = new List<string>();
        var skipped = new List<(string TypeName, string Reason)>();

        foreach (var assembly in assemblies)
        {
            IReadOnlyList<ScannedExtension> scanned;

            try
            {
                scanned = _scanner.Scan(assembly);
            }
            catch (ReflectionTypeLoadException ex)
            {
                skipped.Add((assembly.GetName().Name ?? assembly.FullName ?? "?",
                    $"assembly failed to load its types: {ex.LoaderExceptions.FirstOrDefault()?.Message ?? ex.Message}"));
                continue;
            }

            foreach (var extension in scanned)
            {
                var typeName = extension.ImplementationType.FullName ?? extension.ImplementationType.Name;

                if (TryInstantiate(extension, out var instance, out var failureReason))
                {
                    try
                    {
                        _registry.RegisterExtensionUntyped(extension.ExtensionPointId, instance!);
                        registered.Add(typeName);
                    }
                    catch (Exception ex)
                    {
                        skipped.Add((typeName, $"registration against '{extension.ExtensionPointId}' failed: {ex.Message}"));
                    }
                }
                else
                {
                    skipped.Add((typeName, failureReason!));
                }
            }
        }

        return new ExtensionLoadResult(registered, skipped);
    }

    /// <summary>
    /// Attempts to construct an instance of a scanned extension's implementation
    /// type via its widest public constructor, resolving each parameter from
    /// <see cref="_container"/>. Never throws — failures are reported through
    /// the <paramref name="failureReason"/> out parameter instead, so a single
    /// bad extension can't abort the whole scan.
    /// </summary>
    private bool TryInstantiate(ScannedExtension extension, out object? instance, out string? failureReason)
    {
        if (!_registry.IsExtensionPointDeclared(extension.ExtensionPointId))
        {
            instance = null;
            failureReason = $"extension point '{extension.ExtensionPointId}' has not been declared yet";
            return false;
        }

        var constructors = extension.ImplementationType.GetConstructors();

        if (constructors.Length == 0)
        {
            instance = null;
            failureReason = "has no public constructors";
            return false;
        }

        var constructor = constructors.OrderByDescending(c => c.GetParameters().Length).First();
        var parameters = constructor.GetParameters();
        var arguments = new object?[parameters.Length];

        for (var i = 0; i < parameters.Length; i++)
        {
            var parameterType = parameters[i].ParameterType;

            try
            {
                arguments[i] = _container.GetService(parameterType);
                continue;
            }
            catch (Exception)
            {
                // Fall through to the default-value check below.
            }

            if (parameters[i].HasDefaultValue)
            {
                arguments[i] = parameters[i].DefaultValue;
                continue;
            }

            instance = null;
            failureReason =
                $"constructor parameter '{parameters[i].Name}' of type '{parameterType.FullName}' " +
                "could not be resolved from the service container";
            return false;
        }

        try
        {
            instance = constructor.Invoke(arguments);
            failureReason = null;
            return true;
        }
        catch (Exception ex)
        {
            instance = null;
            failureReason = $"threw during construction: {(ex.InnerException ?? ex).Message}";
            return false;
        }
    }
}

/// <summary>
/// Summary of an <see cref="InternalExtensionLoader.ScanAndRegister"/> call:
/// which extensions were successfully registered, and which were skipped and why.
/// </summary>
public sealed class ExtensionLoadResult
{
    /// <summary>Full type names of extensions successfully constructed and registered.</summary>
    public IReadOnlyList<string> Registered { get; }

    /// <summary>Full type names of extensions that were skipped, paired with why.</summary>
    public IReadOnlyList<(string TypeName, string Reason)> Skipped { get; }

    public ExtensionLoadResult(IReadOnlyList<string> registered, IReadOnlyList<(string TypeName, string Reason)> skipped)
    {
        Registered = registered;
        Skipped = skipped;
    }

    /// <summary>Writes a one-line-per-item summary to the given logger.</summary>
    public void LogTo(ILogger logger)
    {
        logger.Info("Internal extension scan: {0} registered, {1} skipped.", Registered.Count, Skipped.Count);

        foreach (var typeName in Registered)
        {
            logger.Debug("  registered: {0}", typeName);
        }

        foreach (var (typeName, reason) in Skipped)
        {
            logger.Warn("  skipped: {0} — {1}", typeName, reason);
        }
    }
}