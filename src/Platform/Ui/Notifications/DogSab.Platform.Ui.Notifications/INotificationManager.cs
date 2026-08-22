namespace DogSab.Platform.Ui.Notifications;

/// <summary>Shows notifications to the user, independent of how they're visually presented.</summary>
public interface INotificationManager
{
    /// <summary>Raised whenever a notification is shown, for the UI layer to render.</summary>
    event Action<Notification>? NotificationShown;

    /// <summary>Shows a notification.</summary>
    /// <param name="notification">The notification to show.</param>
    void Show(Notification notification);
}