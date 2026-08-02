using System.Text.Json.Serialization;

namespace DogSab.Platform.PluginSystem.Manifest;

/// <summary>
/// Raw deserialization target for a plugin's <c>plugin.json</c> manifest file,
/// mirroring its JSON structure field-for-field with plain string/primitive
/// types. Intentionally dumb — no validation, no parsing of version strings
/// or version ranges happens here. <see cref="PluginManifestParser"/> maps
/// this into a fully-typed <see cref="PluginManifestImpl"/>, where fields like
/// <see cref="Version"/> become real <c>PluginVersion</c> values and errors
/// are reported as <see cref="Diagnostics.PluginManifestParseException"/>
/// rather than silent deserialization failures.
/// </summary>
internal sealed class PluginManifestJsonModel
{
    /// <summary>Maps to <c>IPluginManifest.Id</c> after parsing.</summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>Maps to <c>IPluginManifest.Version</c> after parsing.</summary>
    [JsonPropertyName("version")]
    public string? Version { get; set; }

    /// <summary>Maps to <c>IPluginManifest.DisplayName</c>.</summary>
    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    /// <summary>Maps to <c>IPluginManifest.Description</c>.</summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>Maps to <c>IPluginManifest.Author</c>.</summary>
    [JsonPropertyName("author")]
    public string? Author { get; set; }

    /// <summary>Maps to <c>IPluginManifest.CompatiblePlatformVersionRange</c> after parsing.</summary>
    [JsonPropertyName("compatiblePlatformVersion")]
    public string? CompatiblePlatformVersion { get; set; }

    /// <summary>Maps to <c>IPluginManifest.MainAssemblyFileName</c>.</summary>
    [JsonPropertyName("mainAssembly")]
    public string? MainAssembly { get; set; }

    /// <summary>Raw dependency entries; each maps to a <c>PluginDependencyDescriptor</c> after parsing.</summary>
    [JsonPropertyName("dependsOn")]
    public List<PluginDependencyJsonModel>? DependsOn { get; set; }

    /// <summary>Raw extension entries; each maps to an <c>ExtensionDeclaration</c> after parsing.</summary>
    [JsonPropertyName("extensions")]
    public List<ExtensionDeclarationJsonModel>? Extensions { get; set; }

    /// <summary>Raw requested permission names; each maps to a <c>PluginPermission</c> enum value after parsing.</summary>
    [JsonPropertyName("requestedPermissions")]
    public List<string>? RequestedPermissions { get; set; }
}

/// <summary>Raw deserialization target for a single entry in the manifest's <c>"dependsOn"</c> array.</summary>
internal sealed class PluginDependencyJsonModel
{
    /// <summary>Maps to <c>PluginDependencyDescriptor.DependencyPluginId</c> after parsing.</summary>
    [JsonPropertyName("pluginId")]
    public string? PluginId { get; set; }

    /// <summary>Maps to <c>PluginDependencyDescriptor.AcceptableVersionRange</c> after parsing.</summary>
    [JsonPropertyName("versionRange")]
    public string? VersionRange { get; set; }

    /// <summary>Maps to <c>PluginDependencyDescriptor.IsOptional</c>. Defaults to <c>false</c> if omitted from the JSON.</summary>
    [JsonPropertyName("optional")]
    public bool Optional { get; set; }
}

/// <summary>Raw deserialization target for a single entry in the manifest's <c>"extensions"</c> array.</summary>
internal sealed class ExtensionDeclarationJsonModel
{
    /// <summary>Maps to <c>ExtensionDeclaration.ExtensionPointId</c>.</summary>
    [JsonPropertyName("extensionPoint")]
    public string? ExtensionPoint { get; set; }

    /// <summary>Maps to <c>ExtensionDeclaration.ImplementationClassName</c>.</summary>
    [JsonPropertyName("implementationClass")]
    public string? ImplementationClass { get; set; }
}