namespace DogSab.Platform.Core.Abstractions.Logging;

/// <summary>Severity level of a log entry.</summary>
public enum LogLevel
{
    /// <summary>Fine-grained diagnostic information, useful only during development.</summary>
    Debug,

    /// <summary>Informational messages describing normal operation.</summary>
    Info,

    /// <summary>Messages describing a potential problem that does not stop execution.</summary>
    Warn,

    /// <summary>Messages describing an error that affected an operation.</summary>
    Error,

    /// <summary>Messages describing a critical, typically unrecoverable failure.</summary>
    Fatal
}