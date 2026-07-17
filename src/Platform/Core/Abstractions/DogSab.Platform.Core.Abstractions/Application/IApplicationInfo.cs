namespace DogSab.Platform.Core.Abstractions.Application;

/// <summary>Provides static information about the running application build.</summary>
public interface IApplicationInfo
{
    /// <summary>The application's semantic version.</summary>
    Version Version { get; }

    /// <summary>The internal build number/identifier.</summary>
    string BuildNumber { get; }

    /// <summary>Indicates whether this build is an Early Access Program (pre-release) build.</summary>
    bool IsEap { get; }

    /// <summary>The date on which this build was produced.</summary>
    DateTime BuildDate { get; }
}