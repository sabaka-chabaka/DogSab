using System.Text.Json;
using DogSab.Platform.Extensibility.Abstractions.Compatibility;
using DogSab.Platform.Extensibility.Abstractions.Manifest;
using DogSab.Platform.Extensibility.Abstractions.Sandbox;
using DogSab.Platform.PluginSystem.Diagnostics;

namespace DogSab.Platform.PluginSystem.Manifest;

/// <summary>
/// Reads and validates a plugin's <c>plugin.json</c> manifest file, converting
/// it from raw JSON into a fully-typed <see cref="IPluginManifest"/>. Any
/// malformed JSON, missing required field, or unparsable value (an invalid
/// version string, an unrecognized permission name, etc.) is reported as a
/// <see cref="PluginManifestParseException"/> naming the specific problem,
/// rather than allowing a generic deserialization or null-reference failure
/// to surface without context.
/// </summary>
public sealed class PluginManifestParser
{
    /// <summary>
    /// Reads and parses the manifest file at the given path.
    /// </summary>
    /// <param name="manifestPath">The absolute path to the <c>plugin.json</c> file.</param>
    /// <returns>The fully-typed, validated manifest.</returns>
    /// <exception cref="PluginManifestParseException">
    /// Thrown if the file cannot be read, is not valid JSON, or is missing a
    /// required field or contains an unparsable value.
    /// </exception>
    public IPluginManifest Parse(string manifestPath)
    {
        var jsonModel = ReadAndDeserialize(manifestPath);

        var id = new PluginId(RequireField(manifestPath, jsonModel.Id, "id"));
        var version = ParseVersionField(manifestPath, jsonModel.Version, "version");
        var mainAssembly = RequireField(manifestPath, jsonModel.MainAssembly, "mainAssembly");

        var compatibleRange = string.IsNullOrWhiteSpace(jsonModel.CompatiblePlatformVersion)
            ? VersionRange.Any()
            : ParseVersionRangeField(manifestPath, jsonModel.CompatiblePlatformVersion, "compatiblePlatformVersion");

        var dependencies = MapDependencies(manifestPath, jsonModel.DependsOn);
        var extensions = MapExtensions(manifestPath, jsonModel.Extensions);
        var permissions = MapPermissions(manifestPath, jsonModel.RequestedPermissions);

        return new PluginManifestImpl(
            id,
            version,
            jsonModel.DisplayName ?? id.Value,
            jsonModel.Description ?? string.Empty,
            jsonModel.Author ?? string.Empty,
            compatibleRange,
            dependencies,
            extensions,
            mainAssembly,
            permissions);
    }

    /// <summary>
    /// Reads the manifest file from disk and deserializes it into the raw JSON
    /// model, wrapping any I/O or JSON syntax failure into a
    /// <see cref="PluginManifestParseException"/>.
    /// </summary>
    /// <param name="manifestPath">The path to the manifest file.</param>
    /// <returns>The deserialized raw model.</returns>
    private static PluginManifestJsonModel ReadAndDeserialize(string manifestPath)
    {
        string jsonText;

        try
        {
            jsonText = File.ReadAllText(manifestPath);
        }
        catch (IOException ex)
        {
            throw new PluginManifestParseException(manifestPath, "could not read the manifest file.", ex);
        }

        try
        {
            var model = JsonSerializer.Deserialize<PluginManifestJsonModel>(jsonText);
            return model ?? throw new PluginManifestParseException(manifestPath, "manifest file is empty or 'null'.");
        }
        catch (JsonException ex)
        {
            throw new PluginManifestParseException(manifestPath, "manifest is not valid JSON.", ex);
        }
    }

    /// <summary>
    /// Verifies a required string field is present and non-empty.
    /// </summary>
    /// <param name="manifestPath">The manifest path, used for error reporting.</param>
    /// <param name="value">The raw field value.</param>
    /// <param name="fieldName">The JSON field name, used for error reporting.</param>
    /// <returns>The validated, non-null field value.</returns>
    /// <exception cref="PluginManifestParseException">Thrown if <paramref name="value"/> is null, empty, or whitespace.</exception>
    private static string RequireField(string manifestPath, string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new PluginManifestParseException(manifestPath, $"required field '{fieldName}' is missing or empty.");
        }

        return value;
    }

    /// <summary>
    /// Parses a required version field, reporting a specific error if it is
    /// missing or not a valid version string.
    /// </summary>
    /// <param name="manifestPath">The manifest path, used for error reporting.</param>
    /// <param name="value">The raw field value.</param>
    /// <param name="fieldName">The JSON field name, used for error reporting.</param>
    /// <returns>The parsed version.</returns>
    private static PluginVersion ParseVersionField(string manifestPath, string? value, string fieldName)
    {
        var requiredValue = RequireField(manifestPath, value, fieldName);

        if (!PluginVersion.TryParse(requiredValue, out var version))
        {
            throw new PluginManifestParseException(
                manifestPath,
                $"field '{fieldName}' has an invalid version value '{requiredValue}'. Expected format: Major.Minor.Patch[-PreRelease].");
        }

        return version;
    }

    /// <summary>
    /// Parses a version range field, reporting a specific error if the
    /// expression is malformed.
    /// </summary>
    /// <param name="manifestPath">The manifest path, used for error reporting.</param>
    /// <param name="value">The raw field value.</param>
    /// <param name="fieldName">The JSON field name, used for error reporting.</param>
    /// <returns>The parsed version range.</returns>
    private static VersionRange ParseVersionRangeField(string manifestPath, string value, string fieldName)
    {
        try
        {
            return VersionRange.Parse(value);
        }
        catch (FormatException ex)
        {
            throw new PluginManifestParseException(
                manifestPath,
                $"field '{fieldName}' has an invalid version range expression '{value}'.",
                ex);
        }
    }

    /// <summary>
    /// Maps the manifest's raw dependency entries into typed descriptors.
    /// </summary>
    /// <param name="manifestPath">The manifest path, used for error reporting.</param>
    /// <param name="rawDependencies">The raw dependency entries, or <c>null</c> if the manifest declared none.</param>
    /// <returns>The mapped dependency descriptors, or an empty list if <paramref name="rawDependencies"/> was <c>null</c>.</returns>
    private static IReadOnlyList<PluginDependencyDescriptor> MapDependencies(
        string manifestPath,
        List<PluginDependencyJsonModel>? rawDependencies)
    {
        if (rawDependencies is null)
        {
            return Array.Empty<PluginDependencyDescriptor>();
        }

        var result = new List<PluginDependencyDescriptor>(rawDependencies.Count);

        foreach (var raw in rawDependencies)
        {
            var pluginId = new PluginId(RequireField(manifestPath, raw.PluginId, "dependsOn[].pluginId"));

            var range = string.IsNullOrWhiteSpace(raw.VersionRange)
                ? VersionRange.Any()
                : ParseVersionRangeField(manifestPath, raw.VersionRange, "dependsOn[].versionRange");

            result.Add(new PluginDependencyDescriptor(pluginId, range, raw.Optional));
        }

        return result;
    }

    /// <summary>
    /// Maps the manifest's raw extension entries into typed declarations.
    /// </summary>
    /// <param name="manifestPath">The manifest path, used for error reporting.</param>
    /// <param name="rawExtensions">The raw extension entries, or <c>null</c> if the manifest declared none.</param>
    /// <returns>The mapped extension declarations, or an empty list if <paramref name="rawExtensions"/> was <c>null</c>.</returns>
    private static IReadOnlyList<ExtensionDeclaration> MapExtensions(
        string manifestPath,
        List<ExtensionDeclarationJsonModel>? rawExtensions)
    {
        if (rawExtensions is null)
        {
            return Array.Empty<ExtensionDeclaration>();
        }

        var result = new List<ExtensionDeclaration>(rawExtensions.Count);

        foreach (var raw in rawExtensions)
        {
            var extensionPointId = RequireField(manifestPath, raw.ExtensionPoint, "extensions[].extensionPoint");
            var implementationClass = RequireField(manifestPath, raw.ImplementationClass, "extensions[].implementationClass");

            result.Add(new ExtensionDeclaration(extensionPointId, implementationClass));
        }

        return result;
    }

    /// <summary>
    /// Maps the manifest's raw requested permission names into typed enum values.
    /// </summary>
    /// <param name="manifestPath">The manifest path, used for error reporting.</param>
    /// <param name="rawPermissions">The raw permission name strings, or <c>null</c> if the manifest declared none.</param>
    /// <returns>The mapped permissions, or an empty list if <paramref name="rawPermissions"/> was <c>null</c>.</returns>
    private static IReadOnlyList<PluginPermission> MapPermissions(string manifestPath, List<string>? rawPermissions)
    {
        if (rawPermissions is null)
        {
            return Array.Empty<PluginPermission>();
        }

        var result = new List<PluginPermission>(rawPermissions.Count);

        foreach (var raw in rawPermissions)
        {
            if (!Enum.TryParse<PluginPermission>(raw, ignoreCase: true, out var permission))
            {
                throw new PluginManifestParseException(
                    manifestPath,
                    $"requested permission '{raw}' is not a recognized permission name.");
            }

            result.Add(permission);
        }

        return result;
    }
}