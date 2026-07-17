namespace DogSab.Platform.Core.Abstractions.Logging;

/// <summary>Creates <see cref="ILogger"/> instances scoped to a category.</summary>
public interface ILoggerFactory
{
    /// <summary>
    /// Gets a logger scoped to the given type's full name.
    /// </summary>
    /// <param name="category">The type used to derive the logger's category name.</param>
    /// <returns>A logger scoped to that category.</returns>
    ILogger GetLogger(Type category);

    /// <summary>
    /// Gets a logger scoped to an explicit category name.
    /// </summary>
    /// <param name="category">The category name for the logger.</param>
    /// <returns>A logger scoped to that category.</returns>
    ILogger GetLogger(string category);
}