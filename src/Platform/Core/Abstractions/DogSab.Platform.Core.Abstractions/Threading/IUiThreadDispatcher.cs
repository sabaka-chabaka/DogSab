namespace DogSab.Platform.Core.Abstractions.Threading;

/// <summary>Provides access to and scheduling on the UI (main) thread.</summary>
public interface IUiThreadDispatcher
{
    /// <summary>Indicates whether the calling code is currently running on the UI thread.</summary>
    bool IsUiThread { get; }

    /// <summary>Throws if the calling code is not currently running on the UI thread.</summary>
    void VerifyUiThread();

    /// <summary>
    /// Executes an action synchronously on the UI thread, blocking the caller until it completes.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    void Invoke(Action action);

    /// <summary>
    /// Schedules an action to run on the UI thread and returns a task that completes when it finishes.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <returns>A task representing the scheduled work.</returns>
    Task InvokeAsync(Action action);

    /// <summary>
    /// Schedules a function to run on the UI thread and returns its result asynchronously.
    /// </summary>
    /// <typeparam name="T">The return type of the function.</typeparam>
    /// <param name="func">The function to execute.</param>
    /// <returns>A task producing the function's result.</returns>
    Task<T> InvokeAsync<T>(Func<T> func);
}