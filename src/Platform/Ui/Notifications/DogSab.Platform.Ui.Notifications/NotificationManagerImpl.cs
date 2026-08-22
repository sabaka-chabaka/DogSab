namespace DogSab.Platform.Ui.Notifications;

/// <summary>
/// Default implementation of <see cref="INotificationManager"/>. Purely an
/// event-raising pass-through — actual toast/panel rendering is left to a
/// future <c>Ui.Shell</c>-hosted subscriber, keeping this module free of any
/// Avalonia dependency, consistent with how <c>Ui.ToolWindows.Abstractions</c>
/// avoided depending on Avalonia by using an opaque <c>object Content</c>.
/// </summary>
public sealed class NotificationManagerImpl : INotificationManager
{
    /// <inheritdoc />
    public event Action<Notification>? NotificationShown;

    /// <inheritdoc />
    public void Show(Notification notification)
    {
        NotificationShown?.Invoke(notification);
    }
}