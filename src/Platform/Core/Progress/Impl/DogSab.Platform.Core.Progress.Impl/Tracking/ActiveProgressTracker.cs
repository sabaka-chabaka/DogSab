using DogSab.Platform.Core.Abstractions.Progress;

namespace DogSab.Platform.Core.Progress.Impl.Tracking;

/// <summary>
/// Tracks which <see cref="IProgressIndicator"/>, if any, is associated with
/// the operation currently running on the calling thread, so that deeply
/// nested code (e.g. a PSI visitor called from within an indexing operation)
/// can report progress without the indicator being threaded through every
/// intermediate method call as an explicit parameter.
/// </summary>
public sealed class ActiveProgressTracker
{
    /// <summary>Per-thread reference to the indicator of the operation currently executing on that thread.</summary>
    private readonly AsyncLocal<IProgressIndicator?> _current = new();

    /// <summary>
    /// Sets the active indicator for the calling thread, to be observed by any
    /// code running on this thread until <see cref="Clear"/> is called.
    /// </summary>
    /// <param name="indicator">The indicator to associate with the calling thread.</param>
    public void SetCurrent(IProgressIndicator? indicator)
    {
        _current.Value = indicator;
    }
    
    /// <summary>
    /// Returns the indicator currently associated with the calling thread, if any.
    /// </summary>
    /// <returns>The active indicator, or <c>null</c> if none is set for this thread.</returns>
    public IProgressIndicator? GetCurrent()
    {
        return _current.Value;
    }

    
    /// <summary>
    /// Clears the active indicator for the calling thread. Must be called once
    /// the associated operation finishes, typically in a <c>finally</c> block.
    /// </summary>
    public void Clear()
    {
        _current.Value = null;
    }
}