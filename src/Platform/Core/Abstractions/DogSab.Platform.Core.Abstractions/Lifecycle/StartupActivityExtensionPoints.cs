using DogSab.Platform.Extensibility.Abstractions.ExtensionPoints;

namespace DogSab.Platform.Core.Abstractions.Lifecycle;

public static class StartupActivityExtensionPoints
{
    public static readonly ExtensionPointName<IStartupActivity> STARTUP_ACTIVITY =
        ExtensionPointName<IStartupActivity>.Create(
            "core.startupActivity",
            "Contributes an action executed once during application or project startup.");
}