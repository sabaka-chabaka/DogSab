namespace DogSab.Platform.Core.Abstractions.Logging;

/// <summary>A structured logger scoped to a specific category (typically a type or subsystem name).</summary>
public interface ILogger
{
    /// <summary>
    /// Logs a diagnostic message useful only during development/troubleshooting.
    /// </summary>
    /// <param name="message">The message template.</param>
    /// <param name="args">Values to substitute into the message template.</param>
    void Debug(string message, params object?[] args);

    /// <summary>
    /// Logs a message describing normal operation.
    /// </summary>
    /// <param name="message">The message template.</param>
    /// <param name="args">Values to substitute into the message template.</param>
    void Info(string message, params object?[] args);

    /// <summary>
    /// Logs a message describing a potential problem that does not stop execution.
    /// </summary>
    /// <param name="message">The message template.</param>
    /// <param name="args">Values to substitute into the message template.</param>
    void Warn(string message, params object?[] args);

    /// <summary>
    /// Logs a message describing an error, optionally with the associated exception.
    /// </summary>
    /// <param name="message">The message template.</param>
    /// <param name="exception">The exception associated with the error, if any.</param>
    /// <param name="args">Values to substitute into the message template.</param>
    void Error(string message, Exception? exception = null, params object?[] args);

    /// <summary>
    /// Logs a message describing a critical, typically unrecoverable failure.
    /// </summary>
    /// <param name="message">The message template.</param>
    /// <param name="exception">The exception associated with the failure, if any.</param>
    /// <param name="args">Values to substitute into the message template.</param>
    void Fatal(string message, Exception? exception = null, params object?[] args);

    /// <summary>
    /// Checks whether messages at the given level would actually be recorded.
    /// </summary>
    /// <param name="level">The level to check.</param>
    /// <returns><c>true</c> if logging at this level is enabled; otherwise <c>false</c>.</returns>
    bool IsEnabled(LogLevel level);
}