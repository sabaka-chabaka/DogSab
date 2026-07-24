using DogSab.Platform.Core.Abstractions.Components;
using DogSab.Platform.Core.Abstractions.Project;

namespace DogSab.Platform.Core.Impl.Project;

/// <summary>
/// Default implementation of <see cref="ICurrentProjectAccessor"/>, backed by
/// <see cref="AsyncLocal{T}"/> so the active project ID correctly flows
/// through <c>await</c> continuations, even when they resume on a different
/// thread than the one that entered the scope.
/// </summary>
public sealed class CurrentProjectAccessorImpl : ICurrentProjectAccessor
{
    /// <summary>The ambient, async-flowing storage for the currently active project ID.</summary>
    private readonly AsyncLocal<Guid?> _currentProjectId = new();

    /// <inheritdoc />
    public Guid? CurrentProjectId => _currentProjectId.Value;

    /// <inheritdoc />
    public IDisposable EnterProjectScope(Guid projectId)
    {
        var previousProjectId = _currentProjectId.Value;
        _currentProjectId.Value = projectId;

        return new ProjectScope(this, previousProjectId);
    }

    /// <summary>
    /// Restores the previously active project ID when disposed, allowing
    /// <see cref="EnterProjectScope"/> calls to nest correctly.
    /// </summary>
    private sealed class ProjectScope : IDisposable
    {
        private readonly CurrentProjectAccessorImpl _owner;
        private readonly Guid? _previousProjectId;
        private bool _isDisposed;

        public ProjectScope(CurrentProjectAccessorImpl owner, Guid? previousProjectId)
        {
            _owner = owner;
            _previousProjectId = previousProjectId;
        }
        
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            _owner._currentProjectId.Value = _previousProjectId;
        }
    }
}