using Avalonia.Threading;
using DogSab.Platform.Core.Abstractions.Logging;
using DogSab.Platform.Core.Abstractions.Threading;

namespace DogSab.Platform.Core.Threading.Impl.UiThread;

/// <summary>
/// Default implementation of <see cref="IUiThreadDispatcher"/> backed by
/// Avalonia's <see cref="Dispatcher.UIThread"/>. This is the only type in the
/// platform permitted to reference Avalonia's dispatcher directly — every other
/// subsystem depends only on <see cref="IUiThreadDispatcher"/>.
/// </summary>
public sealed class AvaloniaUiThreadDispatcher : IUiThreadDispatcher
{
    /// <summary>Guard used to implement <see cref="VerifyUiThread"/> with diagnostic logging.</summary>
    private readonly UiThreadGuard _guard;

    /// <summary>
    /// Creates a new dispatcher wrapping Avalonia's UI thread.
    /// </summary>
    /// <param name="loggerFactory">Factory used to obtain a logger for thread-violation diagnostics.</param>
    public AvaloniaUiThreadDispatcher(ILoggerFactory loggerFactory)
    {
        _guard = new UiThreadGuard(loggerFactory, () => Dispatcher.UIThread.CheckAccess());
    }

    /// <inheritdoc />
    public bool IsUiThread => Dispatcher.UIThread.CheckAccess();

    /// <inheritdoc />
    public void VerifyUiThread()
    {
        _guard.Verify();
    }

    /// <summary>
    /// Executes an action synchronously on the UI thread. If already on the UI thread,
    /// runs immediately without dispatching; otherwise blocks the caller until the
    /// dispatched action has completed.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    public void Invoke(Action action)
    {
        if (IsUiThread)
        {
            action();
            return;
        }
        
        Dispatcher.UIThread.InvokeAsync(action);
    }

    /// <summary>
    /// Schedules an action to run on the UI thrtead and returns a task that completes
    /// when it finishes. If already on the UI thread, runs immediately.
    /// </summary>
    /// <param name="action">The action to execute</param>
    /// <returns>A task representing the scheduled work.</returns>
    public Task InvokeAsync(Action action)
    {
        if (IsUiThread)
        {
            action();
            return Task.CompletedTask;
        }

        return Dispatcher.UIThread.InvokeAsync(action).GetTask();
    }
    
    /// <summary>
    /// Schedules a function to run on the UI thread and returns its result asynchronously.
    /// If already on the UI thread, runs immediately.
    /// </summary>
    /// <typeparam name="T">The return type of the function.</typeparam>
    /// <param name="func">The function to execute.</param>
    /// <returns>A task producing the function's result.</returns>
    public Task<T> InvokeAsync<T>(Func<T> func)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            return Task.FromResult(func());
        }

        return Dispatcher.UIThread.InvokeAsync(func).GetTask();
    }
}