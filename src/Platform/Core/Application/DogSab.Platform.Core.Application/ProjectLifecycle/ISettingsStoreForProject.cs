using DogSab.Platform.Core.Abstractions.Settings;

namespace DogSab.Platform.Core.Application.ProjectLifecycle;

/// <summary>
/// Marker alias for an <see cref="ISettingsStore"/> instance rooted at a specific
/// project's directory, as opposed to the application-level store rooted at the
/// global per-user configuration directory. Exists purely to let
/// <see cref="ProjectSession"/> and <see cref="ProjectSessionManager"/> express
/// in their signatures which store they mean, since both share the same
/// <see cref="ISettingsStore"/> interface but are backed by different
/// <c>SettingsPathResolver</c> instances under the hood.
/// </summary>
public interface ISettingsStoreForProject : ISettingsStore
{
}