namespace DogSab.Platform.Core.Impl.Diagnostics;

/// <summary>
/// Minimal, dependency-free logger used internally bo Core.Impl during bootstrap,
/// before the full logging subsystem (<c>Core.Logging.Impl</c>) is available.
/// Writes directly to the console, debug output. Not intended for use by plugins
/// or any code outside the platform's own startup sequence - once the application
/// is running, all code should resolve <c>ILogger</c> through <c>ILoggerFactory</c> instead.
/// </summary>
public static class CoreDiagnosticsLogger
{
    /// <summary>
    /// Writes a debug-level bootstrap message, prefixed with a timestamp and category.
    /// </summary>
    /// <param name="category">The subsystem or type name the message originates from.</param>
    /// <param name="message">The message to write.</param>
    public static void Debug(string category, string message)
    {
        Write("DEBUG", category, message, exception: null);
    }

    /// <summary>
    /// Writes an informational bootstrap message, prefixed with a timestamp and category.
    /// </summary>
    /// <param name="category">The subsystem or type name the message originates from.</param>
    /// <param name="message">The message to write.</param>
    public static void Info(string category, string message)
    {
        Write("INFO", category, message, exception: null);
    }

    /// <summary>
    /// Writes an error-level bootstrap message, optionally with an associated exception.
    /// </summary>
    /// <param name="category">The subsystem or type name the message originates from.</param>
    /// <param name="message">The message to write.</param>
    /// <param name="exception">The exception associated with the error, if any.</param>
    public static void Error(string category, string message, Exception? exception = null)
    {
        Write("ERROR", category, message, exception);
    }
    
    /// <summary>
    /// Formats and writes a single bootstrap log line to the console error stream.
    /// </summary>
    /// <param name="level">The severity label to prefix the line with.</param>
    /// <param name="category">The subsystem or type name the message originates from.</param>
    /// <param name="message">The message to write.</param>
    /// <param name="exception">An optional exception whose details are appended.</param>
    private static void Write(string level, string category, string message, Exception? exception)
    {
        var line = $"[{DateTime.UtcNow:HH:mm:ss.fff}] [{level}] [{category}] {message}";

        Console.Error.WriteLine(line);

        if (exception is not null)
        {
            Console.Error.WriteLine(exception.ToString());
        }
    }
}