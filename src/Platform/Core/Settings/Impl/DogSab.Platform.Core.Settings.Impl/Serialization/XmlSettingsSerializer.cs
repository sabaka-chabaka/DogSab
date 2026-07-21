using System.Xml.Serialization;

namespace DogSab.Platform.Core.Settings.Impl.Serialization;

/// <summary>
/// Serializes and deserializes plain settings objects to and from XML files
/// using <see cref="XmlSerializer"/>. Settings types must be public, have a
/// public parameterless constructor, and expose only public read/write
/// properties or fields — the standard constraints of .NET XML serialization.
/// </summary>
public sealed class XmlSettingsSerializer
{
    /// <summary>
    /// Deserializes a settings instance from the given file path.
    /// </summary>
    /// <typeparam name="T">The settings type to deserialize.</typeparam>
    /// <param name="filePath">The path to read the XML file from.</param>
    /// <returns>The deserialized instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the file's contents could not be deserialized as <typeparamref name="T"/>.</exception>
    public T Deserialize<T>(string filePath) where T : class, new()
    {
        var serializer = new XmlSerializer(typeof(T));

        using var stream = File.OpenRead(filePath);
        var result = serializer.Deserialize(stream) as T;

        return result ?? throw new InvalidOperationException(
            $"Failed to deserialize settings of type '{typeof(T).FullName}' from '{filePath}'.");
    }

    /// <summary>
    /// Serializes a settings instance to the given file path, overwriting any
    /// existing content. Writes to a temporary file first and then replaces the
    /// target atomically, so a crash or concurrent read never observes a
    /// partially written settings file.
    /// </summary>
    /// <typeparam name="T">The settings type to serialize.</typeparam>
    /// <param name="filePath">The path to write the XML file to.</param>
    /// <param name="instance">The instance to serialize.</param>
    public void Serialize<T>(string filePath, T instance) where T : class
    {
        var serializer = new XmlSerializer(typeof(T));
        var tempFilePath = filePath + ".tmp";

        using (var stream = File.Create(tempFilePath))
        {
            serializer.Serialize(stream, instance);
        }

        File.Move(tempFilePath, filePath, overwrite: true);
    }
}